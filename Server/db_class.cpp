#include "db_class.h"
#include "parsing_Json.h"
#include <sstream>
#include <iostream>
#include <cstring> // std::strlen
using json = nlohmann::json;


DBClass::DBClass() {
    conn_ = mysql_init(nullptr);
}

DBClass::~DBClass() {
    closeDB();
}

bool DBClass::connectDB(const std::string& host,
    const std::string& user,
    const std::string& password,
    const std::string& database,
    unsigned int port) {
    if (!mysql_real_connect(conn_, host.c_str(), user.c_str(), password.c_str(),
        database.c_str(), port, nullptr, 0)) {
        std::cerr << "[DB] 연결 실패: " << mysql_error(conn_) << std::endl;
        return false;
    }

    // UTF-8 설정
    if (mysql_set_character_set(conn_, "utf8mb4")) {
        std::cerr << "[DB] 문자셋 설정 실패: " << mysql_error(conn_) << std::endl;
        return false;
    }

    std::cout << "[DB] 연결 성공" << std::endl;
    return true;
}

void DBClass::closeDB() {
    if (conn_) {
        mysql_close(conn_);
        conn_ = nullptr;
    }
}

// 10_0 목표달성 테이블명만 여기서 관리(실제 스키마에 맞게 바꾸기)
static constexpr const char* TBL_PLANNER = "grown_planner";
static constexpr const char* TBL_GOAL = "grown_planner_goal";


// --- 안전 이스케이프 헬퍼 ---
std::string DBClass::escape(MYSQL* conn, const std::string& s) {
    if (!conn_) return s;
    std::string out; out.resize(s.size() * 2 + 1);
    unsigned long len = mysql_real_escape_string(conn_, &out[0], s.c_str(), static_cast<unsigned long>(s.size()));
    out.resize(len);
    return out;
}

// CALL 이후 남아있을 수 있는 결과셋 비움
void DBClass::drainResults() {
    while (true) {
        MYSQL_RES* res = mysql_store_result(conn_);
        if (res) mysql_free_result(res);
        int status = mysql_next_result(conn_);
        if (status > 0) {
            std::cerr << "[DB] next_result 에러: " << mysql_error(conn_) << std::endl;
            break;
        }
        if (status < 0) break; // 끝
    }
}

// 로그인확인
bool DBClass::checkLoginFromDB(const std::string& id, const std::string& pw) {
    if (!conn_) {
        std::cerr << "[DB] 연결 안됨" << std::endl;
        return false;
    }

    std::string query =
        "SELECT COUNT(*) FROM USER_INFO WHERE U_ID='" + id +
        "' AND U_PW='" + pw + "'";

    if (mysql_query(conn_, query.c_str())) {
        std::cerr << "[DB] 쿼리 오류: " << mysql_error(conn_) << std::endl;
        return false;
    }

    MYSQL_RES* res = mysql_store_result(conn_);
    if (!res) {
        std::cerr << "[DB] 결과 없음: " << mysql_error(conn_) << std::endl;
        return false;
    }

    MYSQL_ROW row = mysql_fetch_row(res);
    bool success = (row && atoi(row[0]) > 0);
    mysql_free_result(res);
    return success;
}

// 얼굴인식 로그인
std::string DBClass::findUserByFace(const std::vector<float>& faceVec) {
    const char* query = "SELECT U_ID, U_FACE FROM USER_INFO";
    if (mysql_query(conn_, query)) {
        std::cerr << "[DB] 쿼리 실패: " << mysql_error(conn_) << std::endl;
        return "";
    }

    MYSQL_RES* res = mysql_store_result(conn_);
    if (!res) return "";

    MYSQL_ROW row;
    while ((row = mysql_fetch_row(res))) {
        std::string u_id = row[0];
        std::string dbFaceJson = row[1];

        try {
            json dbVecJson = json::parse(dbFaceJson);
            if (!dbVecJson.is_array() || dbVecJson.size() != faceVec.size())
                continue;

            float sum = 0;
            for (size_t i = 0; i < faceVec.size(); ++i) {
                float diff = dbVecJson[i].get<float>() - faceVec[i];
                sum += diff * diff;
            }

            float distance = std::sqrt(sum);
            if (distance < 0.6f) {  // 기준 유클리디안 거리
                mysql_free_result(res);
                return u_id;
            }
        }
        catch (...) {
            continue;
        }
    }

    mysql_free_result(res);
    return "";
}

// 회원가입 부분
bool DBClass::insertUserToDB(const RequestHandler::Signup& u) {
    if (!conn_) {
        std::cerr << "[DB] 연결 안됨\n";
        return false;
    }
    const std::string query =
        "INSERT INTO USER_INFO "
        "(U_ID, U_PW, U_NAME, U_BIRTH, U_GENDER, U_ADDRESS, U_PHONE, U_FACE) "
        "VALUES (?, ?, ?, ?, ?, ?, ?, ?)";

    MYSQL_STMT* stmt = mysql_stmt_init(conn_);
    if (!stmt) {
        std::cerr << "[DB] stmt init 실패\n";
        return false;
    }

    if (mysql_stmt_prepare(stmt, query.c_str(), (unsigned long)query.size()) != 0) {
        std::cerr << "[DB] stmt prepare 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    // ---------- 문자열 길이 변수(포인터로 넘길 것들) ----------
    unsigned long len_id = (unsigned long)u.id.size();
    unsigned long len_pw = (unsigned long)u.pw.size();     // ⚠️ 실서비스: 평문 금지(반드시 해시)
    unsigned long len_name = (unsigned long)u.name.size();
    unsigned long len_birth = (unsigned long)u.birth.size();
    unsigned long len_addr = (unsigned long)u.address.size();
    unsigned long len_phone = (unsigned long)u.phone.size();

    // ---------- GENDER: TINYINT로 바인딩 (문자열 → 정수 변환) ----------
    // u.gender 가 "0"/"1" 같은 문자열이라고 가정. 숫자 아니면 0으로.
    unsigned char gender_u8 = 0;
    try {
        int gi = std::stoi(u.gender);         // "0"/"1" → 0/1
        if (gi < 0) gi = 0;
        if (gi > 255) gi = 255;
        gender_u8 = static_cast<unsigned char>(gi);
    }
    catch (...) {
        gender_u8 = 0;
    }
    // ---------- FACE: 선택값. 없으면 NULL, 있으면 JSON 문자열 ----------
    std::string faceJson;
    my_bool is_null_face = 0; // MariaDB/MySQL C API에서 사용되는 불리언 타입
    unsigned long len_face = 0;

    if (!u.face.empty()) {
        // vector<float> -> JSON array (예: [0.12, -0.45, ...])
        nlohmann::json jFace = u.face;
        faceJson = jFace.dump();                 // compact JSON
        len_face = (unsigned long)faceJson.size();
        is_null_face = 0;
    }
    else {
        // 얼굴인식 미사용 → NULL 저장
        is_null_face = 1;
        len_face = 0;
    }

    MYSQL_BIND bind[8];
    std::memset(bind, 0, sizeof(bind));

    // 문자열 필드 바인딩 람다 (length 포인터까지 설정)
    auto bind_str = [](MYSQL_BIND& b, const std::string& s, unsigned long* plen) {
        b.buffer_type = MYSQL_TYPE_STRING;
        b.buffer = (void*)s.c_str();
        b.buffer_length = (unsigned long)s.size();
        b.length = plen;                 // 실제 길이 포인터
        };

    // 0 U_ID
    bind_str(bind[0], u.id, &len_id);

    // 1 U_PW  ⚠️ 운영에선 반드시 해시된 값(예: bcrypt 60자)으로 저장
    //    길이 60 초과 시 STRICT 모드에서 "Data too long" 오류 납니다.
    bind_str(bind[1], u.pw, &len_pw);

    // 2 U_NAME
    bind_str(bind[2], u.name, &len_name);

    // 3 U_BIRTH (문자열로 받는 구조)
    bind_str(bind[3], u.birth, &len_birth);

    // 4 U_GENDER (TINYINT로 바인딩)
    bind[4].buffer_type = MYSQL_TYPE_TINY;
    bind[4].is_unsigned = 1;
    bind[4].buffer = &gender_u8;
    bind[4].buffer_length = sizeof(gender_u8);

    // 5 U_ADDRESS
    bind_str(bind[5], u.address, &len_addr);

    // 6 U_PHONE
    bind_str(bind[6], u.phone, &len_phone);

    // 7 U_FACE (선택)
    if (is_null_face) {
        bind[7].buffer_type = MYSQL_TYPE_NULL; // 실제 DB에 NULL로 저장
        bind[7].is_null = &is_null_face;
    }
    else {
        bind[7].buffer_type = MYSQL_TYPE_STRING; // MariaDB JSON은 내부적으로 text 계열
        bind[7].buffer = (void*)faceJson.c_str();
        bind[7].buffer_length = len_face;
        bind[7].length = &len_face;         // 길이 포인터 지정
        bind[7].is_null = &is_null_face;     // false
    }

    //// 얼굴 벡터를 JSON 문자열로 직렬
    //// 예: [0.12, -0.45, ...]
    //nlohmann::json jFace = u.face;         // vector<float> -> json array
    //const std::string faceJson = jFace.dump(); // compact string

    //MYSQL_BIND bind[8];
    //std::memset(bind, 0, sizeof(bind));

    //auto bind_str = [](MYSQL_BIND& b, const std::string& s) {
    //    b.buffer_type = MYSQL_TYPE_STRING;
    //    b.buffer = (void*)s.c_str();
    //    b.buffer_length = (unsigned long)s.size();
    //    };

    //// 문자열 필드 바인딩
    //bind_str(bind[0], u.id);
    //bind_str(bind[1], u.pw);      // ⚠️ 실서비스: 평문 금지(반드시 해시)
    //bind_str(bind[2], u.name);
    //bind_str(bind[3], u.birth);
    //bind_str(bind[4], u.gender);
    //bind_str(bind[5], u.address);
    //bind_str(bind[6], u.phone);

    //// 얼굴 임베딩(JSON 문자열로 저장) — 컬럼 타입이 JSON이면 VALID JSON이어야 함
    //bind_str(bind[7], faceJson);

    //if (mysql_stmt_bind_param(stmt, bind) != 0) {
    //    std::cerr << "[DB] bind 실패: " << mysql_stmt_error(stmt) << "\n";
    //    mysql_stmt_close(stmt);
    //    return false;
    //}

    //if (mysql_stmt_execute(stmt) != 0) {
    //    std::cerr << "[DB] INSERT 실패: " << mysql_stmt_error(stmt) << "\n";
    //    mysql_stmt_close(stmt);
    //    return false;
    //}
    if (mysql_stmt_bind_param(stmt, bind) != 0) {
        std::cerr << "[DB] bind 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    if (mysql_stmt_execute(stmt) != 0) {
        std::cerr << "[DB] INSERT 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }
    mysql_stmt_close(stmt);
    return true;
}

// 아이디 중복확인
bool DBClass::isUserIdExists(const std::string& id) {
    if (!conn_) {
        std::cerr << "[DB] isUserExists 연결 안됨\n";
		return false;
    }

    std::string query = "SELECT COUNT(*) FROM user_info WHERE U_ID = ?";
    
    // 준비된 문장 핸들 생성
    MYSQL_STMT* stmt = mysql_stmt_init(conn_);
    if (!stmt) {
        std::cerr << "[DB] stmt_init 실패\n";
		return false;
    }

    if (mysql_stmt_prepare(stmt, query.c_str(), (unsigned long)query.length()) != 0) {
        std::cerr << "[DB] prepare 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    MYSQL_BIND bind[1] = {};
    memset(bind, 0, sizeof(bind));
    unsigned long id_len = (unsigned long)id.size(); // 실제 길이
    bind[0].buffer_type = MYSQL_TYPE_STRING;
    bind[0].buffer = (void*)id.c_str();
    bind[0].buffer_length = id_len;
    bind[0].length = &id_len;      // 값의 실제 길이

    //mysql_stmt_bind_param(stmt, bind);
    //mysql_stmt_execute(stmt

    if (mysql_stmt_bind_param(stmt, bind) != 0) {
        std::cerr << "[DB] bind_param 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    // 5) 실행
    if (mysql_stmt_execute(stmt) != 0) {
        std::cerr << "[DB] execute 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }
    if (mysql_stmt_store_result(stmt) != 0) {
        std::cerr << "[DB] store_result 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    unsigned long long count = 0;
    MYSQL_BIND resultBind[1] = {};
    memset(resultBind, 0, sizeof(resultBind));

    resultBind[0].buffer_type = MYSQL_TYPE_LONGLONG;
    resultBind[0].buffer = (char*)&count;
    resultBind[0].is_unsigned = 1;

    //mysql_stmt_bind_result(stmt, resultBind);
    //mysql_stmt_fetch(stmt);
    //mysql_stmt_close(stmt);

    if (mysql_stmt_bind_result(stmt, resultBind) != 0) {
        std::cerr << "[DB] bind_result 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_free_result(stmt);
        mysql_stmt_close(stmt);
        return false;
    }

    int f = mysql_stmt_fetch(stmt);
    if (f != 0 && f != MYSQL_DATA_TRUNCATED) { // 0=정상
        std::cerr << "[DB] fetch 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_free_result(stmt);
        mysql_stmt_close(stmt);
        return false;
    }
    mysql_stmt_free_result(stmt);  // ★ 버퍼 정리
    mysql_stmt_close(stmt);

    return count > 0;
}

// 종합분석결과 저장
bool DBClass::insertUserTotalResult(const RequestHandler::TotalResult& r)
{
    if (!conn_) {
        std::cerr << "[DB] 연결 안됨\n";
        return false;
    }

    const char* sql =
        "INSERT INTO USER_TOTAL_RESULT (U_ID, RES_CON, RES_ALIVE, RES_ACT, STATUS) "
        "VALUES (?, ?, ?, ?, ?)";

    MYSQL_STMT* stmt = mysql_stmt_init(conn_);
    if (!stmt) { std::cerr << "[DB] stmt_init 실패\n"; return false; }

    if (mysql_stmt_prepare(stmt, sql, (unsigned long)std::strlen(sql)) != 0) {
        std::cerr << "[DB] stmt_prepare 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    MYSQL_BIND bind[5]{};
    unsigned long len_u_id = (unsigned long)r.u_id.size();
    unsigned long len_res_con = (unsigned long)r.res_con.size();
    unsigned long len_res_alive = (unsigned long)r.res_alive.size();
    unsigned long len_res_act = (unsigned long)r.res_act.size();
    signed char status_i8 = r.status ? 1 : 0;

    // U_ID
    bind[0].buffer_type = MYSQL_TYPE_STRING;
    bind[0].buffer = (void*)r.u_id.c_str();
    bind[0].buffer_length = len_u_id;
    bind[0].length = &len_u_id;

    // RES_CON (TEXT)
    bind[1].buffer_type = MYSQL_TYPE_STRING;
    bind[1].buffer = (void*)r.res_con.c_str();
    bind[1].buffer_length = len_res_con;
    bind[1].length = &len_res_con;

    // RES_ALIVE
    bind[2].buffer_type = MYSQL_TYPE_STRING;
    bind[2].buffer = (void*)r.res_alive.c_str();
    bind[2].buffer_length = len_res_alive;
    bind[2].length = &len_res_alive;

    // RES_ACT (ENUM 문자열)
    bind[3].buffer_type = MYSQL_TYPE_STRING;
    bind[3].buffer = (void*)r.res_act.c_str();
    bind[3].buffer_length = len_res_act;
    bind[3].length = &len_res_act;

    // STATUS (TINYINT)
    bind[4].buffer_type = MYSQL_TYPE_TINY;
    bind[4].buffer = &status_i8;

    if (mysql_stmt_bind_param(stmt, bind) != 0) {
        std::cerr << "[DB] bind_param 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    if (mysql_stmt_execute(stmt) != 0) {
        std::cerr << "[DB] execute 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    mysql_stmt_close(stmt);
    return true;
}


static const char* TABLE_RECO_JOBS = "user_res_jobs";
//  USER_TOTAL_RESULT 저장하고 방금 생성된 RES_ID 반환
bool DBClass::insertUserTotalResultWithId(const RequestHandler::TotalResult& r, uint64_t& out_res_id)
{
    out_res_id = 0;
    if (!conn_) { std::cerr << "[DB] 연결 안됨\n"; return false; }

    // ★ 스키마 반영: USER_TOTAL_RESULT는 현재 U_ID, RES_CON만 존재
    const char* sql =
        "INSERT INTO USER_TOTAL_RESULT (U_ID, RES_CON) VALUES (?, ?)";

    MYSQL_STMT* stmt = mysql_stmt_init(conn_);
    if (!stmt) { std::cerr << "[DB] stmt_init 실패\n"; return false; }

    if (mysql_stmt_prepare(stmt, sql, (unsigned long)std::strlen(sql)) != 0) {
        std::cerr << "[DB] stmt_prepare 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    // ★ 바인딩도 2개만
    MYSQL_BIND bind[2]{};
    unsigned long len_uid = (unsigned long)r.u_id.size();
    unsigned long len_con = (unsigned long)r.res_con.size();

    // U_ID
    bind[0].buffer_type = MYSQL_TYPE_STRING;
    bind[0].buffer = (void*)r.u_id.c_str();
    bind[0].buffer_length = len_uid;
    bind[0].length = &len_uid;

    // RES_CON (TEXT)
    bind[1].buffer_type = MYSQL_TYPE_STRING;
    bind[1].buffer = (void*)r.res_con.c_str();
    bind[1].buffer_length = len_con;
    bind[1].length = &len_con;

    if (mysql_stmt_bind_param(stmt, bind) != 0) {
        std::cerr << "[DB] bind_param 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    if (mysql_stmt_execute(stmt) != 0) {
        std::cerr << "[DB] execute 실패: " << mysql_stmt_error(stmt)
            << " | U_ID=" << r.u_id
            << " | RES_CON.len=" << len_con << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    // 방금 INSERT된 PK
    out_res_id = (uint64_t)mysql_insert_id(conn_);

    mysql_stmt_close(stmt);
    return out_res_id != 0;
}

//  USER_RES_JOBS에 recommended_jobs 저장
bool DBClass::insertRecommendedJobs(uint64_t res_id, const std::vector<RequestHandler::RecommendedJob>& jobs)
{
    if (!conn_) { std::cerr << "[DB] 연결 안됨\n"; return false; }
    if (jobs.empty()) return true; // 저장할 게 없으면 성공 취급

    // URJ_ID는 AUTO_INCREMENT → 컬럼 나열하지 않음
    std::string sql = std::string("INSERT INTO ") + TABLE_RECO_JOBS +
        " (JOB, JOB_EXPLAIN, JOB_REASON, RES_ID) VALUES (?, ?, ?, ?)";

    MYSQL_STMT* stmt = mysql_stmt_init(conn_);
    if (!stmt) { std::cerr << "[DB] stmt_init 실패\n"; return false; }

    if (mysql_stmt_prepare(stmt, sql.c_str(), (unsigned long)sql.size()) != 0) {
        std::cerr << "[DB] stmt_prepare 실패: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt); return false;
    }

    MYSQL_BIND bind[4]{};
    unsigned long len_job = 0, len_exp = 0, len_reason = 0;
    unsigned long long res_id_u64 = res_id;

    // 고정 바인딩: 길이/버퍼는 루프에서 매번 갱신
    bind[0].buffer_type = MYSQL_TYPE_STRING; bind[0].length = &len_job;    // JOB
    bind[1].buffer_type = MYSQL_TYPE_STRING; bind[1].length = &len_exp;    // JOB_EXPLAIN
    bind[2].buffer_type = MYSQL_TYPE_STRING; bind[2].length = &len_reason; // JOB_REASON
    bind[3].buffer_type = MYSQL_TYPE_LONGLONG; bind[3].is_unsigned = 1; bind[3].buffer = &res_id_u64; // RES_ID

    for (const auto& it : jobs) {
        // 🔒 컬럼 길이(200/300/300) 초과 방지 — SQL_MODE=STRICT에서 에러 방지
        const std::string job = it.job.substr(0, 200);
        const std::string description = it.description.substr(0, 300);
        const std::string reason = it.reason.substr(0, 300);

        // 완전 빈 레코드는 스킵
        if (job.empty() && description.empty() && reason.empty()) continue;

        bind[0].buffer = (void*)job.c_str();         len_job = (unsigned long)job.size();
        bind[1].buffer = (void*)description.c_str(); len_exp = (unsigned long)description.size();
        bind[2].buffer = (void*)reason.c_str();      len_reason = (unsigned long)reason.size();

        if (mysql_stmt_bind_param(stmt, bind) != 0) {
            std::cerr << "[DB] bind_param 실패: " << mysql_stmt_error(stmt) << "\n";
            mysql_stmt_close(stmt); return false;
        }
        if (mysql_stmt_execute(stmt) != 0) {
            std::cerr << "[DB] execute 실패: " << mysql_stmt_error(stmt) << "\n";
            mysql_stmt_close(stmt); return false;
        }
    }

    mysql_stmt_close(stmt);
    return true;
}

// 이력서관리 저장
bool DBClass::upsertUserCV(const RequestHandler::Resume& cv, uint64_t& out_cv_id)
{
    out_cv_id = 0;
    if (!conn_) { std::cerr << "[DB] 연결 안됨\n"; return false; }

    // 트랜잭션 시작 (둘 중 하나라도 실패하면 롤백)
    mysql_autocommit(conn_, 0);

    // 1) 해당 U_ID의 기존 CV_ID 조회 (한 유저 1건 가정)
    MYSQL_STMT* stmt_sel = mysql_stmt_init(conn_);
    const char* sql_sel = "SELECT CV_ID FROM USER_CV WHERE U_ID=? LIMIT 1";
    if (!stmt_sel || mysql_stmt_prepare(stmt_sel, sql_sel, (unsigned long)std::strlen(sql_sel)) != 0) {
        std::cerr << "[DB] SELECT prepare 실패: " << mysql_stmt_error(stmt_sel) << "\n";
        if (stmt_sel) mysql_stmt_close(stmt_sel);
        mysql_rollback(conn_);
        mysql_autocommit(conn_, 1);
        return false;
    }
    MYSQL_BIND sel_bind[1]{};
    unsigned long len_uid = (unsigned long)cv.u_id.size();
    sel_bind[0].buffer_type = MYSQL_TYPE_STRING;
    sel_bind[0].buffer = (void*)cv.u_id.c_str();
    sel_bind[0].buffer_length = len_uid;
    sel_bind[0].length = &len_uid;
    if (mysql_stmt_bind_param(stmt_sel, sel_bind) != 0 || mysql_stmt_execute(stmt_sel) != 0) {
        std::cerr << "[DB] SELECT 실행 실패: " << mysql_stmt_error(stmt_sel) << "\n";
        mysql_stmt_close(stmt_sel);
        mysql_rollback(conn_);
        mysql_autocommit(conn_, 1);
        return false;
    }
    // 결과 바인딩
    MYSQL_BIND sel_out[1]{};
    unsigned long long cv_id_u64 = 0;
    sel_out[0].buffer_type = MYSQL_TYPE_LONGLONG;
    sel_out[0].buffer = &cv_id_u64;
    if (mysql_stmt_bind_result(stmt_sel, sel_out) != 0) {
        std::cerr << "[DB] SELECT bind_result 실패: " << mysql_stmt_error(stmt_sel) << "\n";
        mysql_stmt_close(stmt_sel);
        mysql_rollback(conn_);
        mysql_autocommit(conn_, 1);
        return false;
    }
    bool exist = (mysql_stmt_fetch(stmt_sel) == 0);
    mysql_stmt_close(stmt_sel);

    // 입력 길이 안전컷 (DB 제한)
    std::string name = cv.name.substr(0, 50);
    std::string email = cv.email.substr(0, 254);
    std::string phone = cv.phone.substr(0, 20);
    std::string birth = cv.birth; // "YYYY-MM-DD" 가정
    std::string addr = cv.address.substr(0, 100);
    std::string career = cv.career.substr(0, 300);
    std::string signi = cv.signi.substr(0, 1000);

    bool ok = false;

    if (exist) {
        // 2-A) UPDATE
        MYSQL_STMT* stmt_upd = mysql_stmt_init(conn_);
        const char* sql_upd =
            "UPDATE USER_CV "
            "SET CV_NAME=?, CV_EMAIL=?, CV_PHONE=?, CV_BIRTH=?, CV_ADDRESS=?, CV_CAREER=?, CV_SIGNI=? "
            "WHERE CV_ID=?";
        if (!stmt_upd || mysql_stmt_prepare(stmt_upd, sql_upd, (unsigned long)std::strlen(sql_upd)) != 0) {
            std::cerr << "[DB] UPDATE prepare 실패: " << mysql_stmt_error(stmt_upd) << "\n";
            if (stmt_upd) mysql_stmt_close(stmt_upd);
            mysql_rollback(conn_); mysql_autocommit(conn_, 1);
            return false;
        }

        MYSQL_BIND b[8]{};
        unsigned long l_name = (unsigned long)name.size(), l_email = (unsigned long)email.size();
        unsigned long l_phone = (unsigned long)phone.size(), l_birth = (unsigned long)birth.size();
        unsigned long l_addr = (unsigned long)addr.size(), l_career = (unsigned long)career.size();
        unsigned long l_signi = (unsigned long)signi.size();
        unsigned long long cv_id_param = cv_id_u64;

        b[0].buffer_type = MYSQL_TYPE_STRING; b[0].buffer = (void*)name.c_str();   b[0].buffer_length = l_name;  b[0].length = &l_name;
        b[1].buffer_type = MYSQL_TYPE_STRING; b[1].buffer = (void*)email.c_str();  b[1].buffer_length = l_email; b[1].length = &l_email;
        b[2].buffer_type = MYSQL_TYPE_STRING; b[2].buffer = (void*)phone.c_str();  b[2].buffer_length = l_phone; b[2].length = &l_phone;
        b[3].buffer_type = MYSQL_TYPE_STRING; b[3].buffer = (void*)birth.c_str();  b[3].buffer_length = l_birth; b[3].length = &l_birth;
        b[4].buffer_type = MYSQL_TYPE_STRING; b[4].buffer = (void*)addr.c_str();   b[4].buffer_length = l_addr;  b[4].length = &l_addr;
        b[5].buffer_type = MYSQL_TYPE_STRING; b[5].buffer = (void*)career.c_str(); b[5].buffer_length = l_career;b[5].length = &l_career;
        b[6].buffer_type = MYSQL_TYPE_STRING; b[6].buffer = (void*)signi.c_str();  b[6].buffer_length = l_signi; b[6].length = &l_signi;
        b[7].buffer_type = MYSQL_TYPE_LONGLONG; b[7].buffer = &cv_id_param; b[7].is_unsigned = 1;

        if (mysql_stmt_bind_param(stmt_upd, b) != 0 || mysql_stmt_execute(stmt_upd) != 0) {
            std::cerr << "[DB] UPDATE 실행 실패: " << mysql_stmt_error(stmt_upd) << "\n";
            mysql_stmt_close(stmt_upd); mysql_rollback(conn_); mysql_autocommit(conn_, 1); return false;
        }
        mysql_stmt_close(stmt_upd);
        out_cv_id = (uint64_t)cv_id_u64;
        ok = true;
    }
    else {
        // 2-B) INSERT
        MYSQL_STMT* stmt_ins = mysql_stmt_init(conn_);
        const char* sql_ins =
            "INSERT INTO USER_CV "
            "(U_ID, CV_NAME, CV_EMAIL, CV_PHONE, CV_BIRTH, CV_ADDRESS, CV_CAREER, CV_SIGNI) "
            "VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
        if (!stmt_ins || mysql_stmt_prepare(stmt_ins, sql_ins, (unsigned long)std::strlen(sql_ins)) != 0) {
            std::cerr << "[DB] INSERT prepare 실패: " << mysql_stmt_error(stmt_ins) << "\n";
            if (stmt_ins) mysql_stmt_close(stmt_ins);
            mysql_rollback(conn_); mysql_autocommit(conn_, 1); return false;
        }

        MYSQL_BIND b[8]{};
        unsigned long l_uid = (unsigned long)cv.u_id.size(), l_name = (unsigned long)name.size(), l_email = (unsigned long)email.size();
        unsigned long l_phone = (unsigned long)phone.size(), l_birth = (unsigned long)birth.size(), l_addr = (unsigned long)addr.size();
        unsigned long l_career = (unsigned long)career.size(), l_signi = (unsigned long)signi.size();

        b[0].buffer_type = MYSQL_TYPE_STRING; b[0].buffer = (void*)cv.u_id.c_str(); b[0].buffer_length = l_uid;   b[0].length = &l_uid;
        b[1].buffer_type = MYSQL_TYPE_STRING; b[1].buffer = (void*)name.c_str();    b[1].buffer_length = l_name;  b[1].length = &l_name;
        b[2].buffer_type = MYSQL_TYPE_STRING; b[2].buffer = (void*)email.c_str();   b[2].buffer_length = l_email; b[2].length = &l_email;
        b[3].buffer_type = MYSQL_TYPE_STRING; b[3].buffer = (void*)phone.c_str();   b[3].buffer_length = l_phone; b[3].length = &l_phone;
        b[4].buffer_type = MYSQL_TYPE_STRING; b[4].buffer = (void*)birth.c_str();   b[4].buffer_length = l_birth; b[4].length = &l_birth;
        b[5].buffer_type = MYSQL_TYPE_STRING; b[5].buffer = (void*)addr.c_str();    b[5].buffer_length = l_addr;  b[5].length = &l_addr;
        b[6].buffer_type = MYSQL_TYPE_STRING; b[6].buffer = (void*)career.c_str();  b[6].buffer_length = l_career;b[6].length = &l_career;
        b[7].buffer_type = MYSQL_TYPE_STRING; b[7].buffer = (void*)signi.c_str();   b[7].buffer_length = l_signi; b[7].length = &l_signi;

        if (mysql_stmt_bind_param(stmt_ins, b) != 0 || mysql_stmt_execute(stmt_ins) != 0) {
            std::cerr << "[DB] INSERT 실행 실패: " << mysql_stmt_error(stmt_ins) << "\n";
            mysql_stmt_close(stmt_ins); mysql_rollback(conn_); mysql_autocommit(conn_, 1); return false;
        }
        out_cv_id = (uint64_t)mysql_insert_id(conn_);
        mysql_stmt_close(stmt_ins);
        ok = (out_cv_id != 0);
    }

    if (ok) mysql_commit(conn_);
    else    mysql_rollback(conn_);
    mysql_autocommit(conn_, 1);
    return ok;
}

// 이력서관리 조회
bool DBClass::getUserCVByUid(const std::string& u_id, ResumeRow& out)
{
    const char* SQL =
        "SELECT CV_ID, CV_NAME, CV_EMAIL, CV_PHONE, "
        "       DATE_FORMAT(CV_BIRTH, '%Y-%m-%d') AS CV_BIRTH, "
        "       CV_ADDRESS, CV_CAREER, CV_SIGNI "
        "FROM USER_CV WHERE U_ID=? "
        "ORDER BY CV_ID DESC LIMIT 1";

    MYSQL_STMT* stmt = mysql_stmt_init(conn_);
    if (!stmt) return false;
    if (mysql_stmt_prepare(stmt, SQL, (unsigned long)strlen(SQL)) != 0) {
        std::cerr << "[DB] prepare failed: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    // param
    MYSQL_BIND p[1]{};
    p[0].buffer_type = MYSQL_TYPE_STRING;
    p[0].buffer = (void*)u_id.c_str();
    p[0].buffer_length = (unsigned long)u_id.size();
    if (mysql_stmt_bind_param(stmt, p) || mysql_stmt_execute(stmt)) {
        std::cerr << "[DB] exec failed: " << mysql_stmt_error(stmt) << "\n";
        mysql_stmt_close(stmt);
        return false;
    }

    // result
    unsigned long cv_id_ul = 0;
    char name[128] = {}, email[256] = {}, phone[32] = {}, birth[16] = {};
    char address[128] = {}, career[512] = {}, signi[2048] = {};
    unsigned long ln = 0, le = 0, lp = 0, lb = 0, la = 0, lc = 0, ls = 0;

    MYSQL_BIND r[8]{};
    r[0].buffer_type = MYSQL_TYPE_LONG; r[0].buffer = &cv_id_ul;
    auto S = [&](MYSQL_BIND& b, char* buf, unsigned long sz, unsigned long* L) {
        memset(&b, 0, sizeof(b)); b.buffer_type = MYSQL_TYPE_STRING; b.buffer = buf; b.buffer_length = sz; b.length = L;
        };
    S(r[1], name, sizeof(name), &ln); S(r[2], email, sizeof(email), &le);
    S(r[3], phone, sizeof(phone), &lp); S(r[4], birth, sizeof(birth), &lb);
    S(r[5], address, sizeof(address), &la); S(r[6], career, sizeof(career), &lc);
    S(r[7], signi, sizeof(signi), &ls);

    if (mysql_stmt_bind_result(stmt, r) || mysql_stmt_fetch(stmt) != 0) {
        mysql_stmt_close(stmt); return false; // not found or error
    }

    out.cv_id = cv_id_ul; out.u_id = u_id;
    out.name = std::string(name, ln); out.email = std::string(email, le);
    out.phone = std::string(phone, lp); out.birth = std::string(birth, lb);
    out.address = std::string(address, la); out.career = std::string(career, lc);
    out.signi = std::string(signi, ls);
    mysql_stmt_close(stmt);
    return true;
}


// ------------- SP: get_user_job_latest_res -------------
bool DBClass::getUserJobsLatestRes(const std::string& u_id, std::string& out_json) {
    if (!conn_) return false;
    std::cout << "잡스 1"<< endl;

    if (mysql_query(conn_, "SET @p_result = NULL;")) {
        std::cerr << "[DB] SET @p_result 실패: " << mysql_error(conn_) << std::endl;
        return false;
    }
    std::cout << "잡스 2" << endl;

    std::string call = "CALL get_user_jobs_latest_res('" + escape(conn_, u_id) + "', @p_result);";
    if (mysql_query(conn_, call.c_str())) {
        std::cerr << "[DB] CALL 실패: " << mysql_error(conn_) << std::endl;
        return false;
    }
    drainResults();
    std::cout << "잡스 3" << endl;

    if (mysql_query(conn_, "SELECT @p_result;")) {
        std::cerr << "[DB] SELECT @p_result 실패: " << mysql_error(conn_) << std::endl;
        return false;
    }
    std::cout << "잡스 4" << endl;

    MYSQL_RES* res = mysql_store_result(conn_);
    if (!res) {
        std::cerr << "[DB] 결과 없음: " << mysql_error(conn_) << std::endl;
        return false;
    }
    std::cout << "잡스 5" << endl;

    MYSQL_ROW row = mysql_fetch_row(res);
    out_json = (row && row[0]) ? row[0] : R"({"RES_ID":null,"items":[]})";
    mysql_free_result(res);
    return true;
}

// ------------- SP: insertGrownPlannerWithGoals -------------
bool DBClass::insertGrownPlannerWithGoals(const std::string& u_id, int period,
    const std::string& job,
    std::string& out_json, std::string& out_err) {
    if (!conn_) { out_err = "no db connection"; return false; }

    // ★ 추가: 신(4파라미터) 프로시저 경로
    // - out_json 이 비어있지 않다면, 호출자가 goal_json 을 담아 넘긴 것으로 간주
    // - InsertGrownPlannerWithGoals(p_u_id, p_period, p_want_job, p_goal_json) 호출
    if (!out_json.empty()) {
        // goal_json 포함 신규 경로
        const std::string eu = escape(conn_, u_id);
        const std::string ejob = escape(conn_, job);
        const std::string egoals = escape(conn_, out_json);  // JSON도 문자열 이스케이프

        std::string callNew =
            "CALL InsertGrownPlannerWithGoals('"
            + eu + "', " + std::to_string(period) + ", '"
            + ejob + "', '" + egoals + "');";

        if (mysql_query(conn_, callNew.c_str())) {
            out_err = mysql_error(conn_);
            std::cerr << "[DB] CALL(InsertGrownPlannerWithGoals) 실패: " << out_err << std::endl;
            return false;
        }
        // [ADDED] SELECT out_json 결과 한 컬럼 읽어서 out_json에 덮어쓰기
        MYSQL_RES* res = mysql_store_result(conn_);
        if (res) {
            MYSQL_ROW row = mysql_fetch_row(res);
            if (row && row[0]) out_json = row[0];
            else               out_json = R"({"status":"ok_no_select"})";
            mysql_free_result(res);
        }
        else {
            // 결과셋이 없을 수도 있으니 기본 마커
            out_json = R"({"status":"ok_no_result"})";
        }
        // 프로시저 잔여 결과 정리
        drainResults();

        // 신규 SP는 @p_result 를 반환하지 않으므로, 최소한 OK 마커를 out_json에 남김
        // (호출부가 out_json을 응답 본문으로 쓰지 않는다면 이 값은 무시해도 됨)
        //out_json = R"({"result":"ok"})";
        return true;
    }

    // ===== [FIX] 레거시(@p_result) 분기 제거 → 4-인자 SP로 우회 호출 =====
    // [REMOVED] SET @p_result / 3-인자 CALL / SELECT @p_result 패턴
    // [ADDED] 최소 입력 JSON 구성 (period, want_job, 빈 goal_json)
    std::string min_json =
        std::string("{\"period\":") + std::to_string(period) +
        ",\"want_job\":\"" + job + "\",\"goal_json\":[]}";
    const std::string ejson = escape(conn_, min_json);  // [ADDED] SQL 문자열 이스케이프

    // [FIX] 4-인자 SP로 호출 (대소문자 포함 정확한 SP명 사용)
    std::string call =
        "CALL InsertGrownPlannerWithGoals('"
        + escape(conn_, u_id) + "', "
        + std::to_string(period) + ", '"
        + escape(conn_, job) + "', '"
        + ejson + "');";
    if (mysql_query(conn_, call.c_str())) {
        out_err = mysql_error(conn_);
        std::cerr << "[DB] CALL(InsertGrownPlannerWithGoals) 실패: " << out_err << std::endl;
        return false;
    }

    // [ADDED] SELECT out_json 한 컬럼 수신 → out_json 덮어쓰기
    MYSQL_RES* res = mysql_store_result(conn_);
    if (!res) {
        out_err = mysql_error(conn_);
        std::cerr << "[DB] 결과 없음: " << out_err << std::endl;
        return false;
    }
    MYSQL_ROW row = mysql_fetch_row(res);
    out_json = (row && row[0]) ? row[0] : R"({"status":"empty_result"})";
    mysql_free_result(res);

    // [KEEP] 잔여 결과 정리
    drainResults();

    return true;

    //const std::string& job,
    //std::string& out_json, std::string& out_err) {
    //if (!conn_) { out_err = "no db connection"; return false; }

    //// ★ 추가: 신(4파라미터) 프로시저 경로
    //// - out_json 이 비어있지 않다면, 호출자가 goal_json 을 담아 넘긴 것으로 간주
    //// - InsertGrownPlannerWithGoals(p_u_id, p_period, p_want_job, p_goal_json) 호출
    //if (!out_json.empty()) {
    //    // goal_json 포함 신규 경로
    //    const std::string eu = escape(conn_, u_id);
    //    const std::string ejob = escape(conn_, job);
    //    const std::string egoals = escape(conn_, out_json);  // JSON도 문자열 이스케이프

    //    std::string callNew =
    //        "CALL InsertGrownPlannerWithGoals('"
    //        + eu + "', " + std::to_string(period) + ", '"
    //        + ejob + "', '" + egoals + "');";

    //    if (mysql_query(conn_, callNew.c_str())) {
    //        out_err = mysql_error(conn_);
    //        std::cerr << "[DB] CALL(InsertGrownPlannerWithGoals) 실패: " << out_err << std::endl;
    //        return false;
    //    }
    //    // [ADDED] SELECT out_json 결과 한 컬럼 읽어서 out_json에 덮어쓰기
    //    MYSQL_RES* res = mysql_store_result(conn_);                 
    //    if (res) {                                                   
    //        MYSQL_ROW row = mysql_fetch_row(res);                   
    //        if (row && row[0]) out_json = row[0];                    
    //        else               out_json = R"({"status":"ok_no_select"})"; 
    //        mysql_free_result(res);                                   
    //    }
    //    else {
    //        // 결과셋이 없을 수도 있으니 기본 마커
    //        out_json = R"({"status":"ok_no_result"})";            
    //    }
    //    // 프로시저 잔여 결과 정리
    //    drainResults();

    //    // 신규 SP는 @p_result 를 반환하지 않으므로, 최소한 OK 마커를 out_json에 남김
    //    // (호출부가 out_json을 응답 본문으로 쓰지 않는다면 이 값은 무시해도 됨)
    //    //out_json = R"({"result":"ok"})";
    //    return true;
    //}

    //if (mysql_query(conn_, "SET @p_result = NULL;")) {
    //    std::cerr << "[DB] SET @p_result 실패: " << mysql_error(conn_) << std::endl;
    //    return false;
    //}

    //std::string call = "CALL insertGrownPlannerWithGoals('"
    //    + escape(conn_, u_id) + "','" + escape(conn_, job) + "', @p_result);";
    //if (mysql_query(conn_, call.c_str())) {
    //    std::cerr << "[DB] CALL 실패: " << mysql_error(conn_) << std::endl;
    //    return false;
    //}
    //drainResults();

    //if (mysql_query(conn_, "SELECT @p_result;")) {
    //    std::cerr << "[DB] SELECT @p_result 실패: " << mysql_error(conn_) << std::endl;
    //    return false;
    //}
    //MYSQL_RES* res = mysql_store_result(conn_);
    //if (!res) {
    //    std::cerr << "[DB] 결과 없음: " << mysql_error(conn_) << std::endl;
    //    return false;
    //}
    //MYSQL_ROW row = mysql_fetch_row(res);
    //out_json = (row && row[0]) ? row[0] : "{}";
    //mysql_free_result(res);
    //return true;
}

//// ------------- SP: GetGrownPlannerWithGoalslatest -------------
//bool DBClass::getGrownPlannerWithGoalsLatest(const std::string& u_id,
//    std::string& out_json) {
//    if (!conn_) return false;
//
//    if (mysql_query(conn_, "SET @p_result = NULL;")) {
//        std::cerr << "[DB] SET @p_result 실패: " << mysql_error(conn_) << std::endl;
//        return false;
//    }
//
//    std::string call = "CALL GetGrownPlannerWithGoalslatest('" + escape(conn_, u_id) + "', @p_result);";
//    if (mysql_query(conn_, call.c_str())) {
//        std::cerr << "[DB] CALL 실패: " << mysql_error(conn_) << std::endl;
//        return false;
//    }
//    drainResults();
//
//    if (mysql_query(conn_, "SELECT @p_result;")) {
//        std::cerr << "[DB] SELECT @p_result 실패: " << mysql_error(conn_) << std::endl;
//        return false;
//    }
//    MYSQL_RES* res = mysql_store_result(conn_);
//    if (!res) {
//        std::cerr << "[DB] 결과 없음: " << mysql_error(conn_) << std::endl;
//        return false;
//    }
//    MYSQL_ROW row = mysql_fetch_row(res);
//    out_json = (row && row[0]) ? row[0] : "{}";
//    mysql_free_result(res);
//    return true;
//}

// ===== [ADD] 최신 성장플래너 + 목표목록 조회
bool DBClass::fetchLatestGrowPlanner(
    const std::string& u_id,
    GrowPlannerHeader& outHeader,
    std::vector<GrowPlannerGoal>& outGoals
) {
    if (!conn_) {
        std::cerr << "[DB] fetchLatestGrowPlanner: conn_ is null\n";
        return false;
    }

    // U_ID 이스케이프 (전역 escape 사용)
    const std::string uid = escape(conn_, u_id);

    // 스샷과 동일한 결과: 최근 GROWN_ID 1건의 헤더 + 목표 목록
    std::ostringstream oss;
    oss <<
        "SELECT gp.GROWN_ID, gp.WANT_JOB, gp.PERIOD, "
        "       gg.GOAL, DATE_FORMAT(gg.GOAL_DATE, '%Y-%m-%d') AS GOAL_DATE, "
        "       gg.CATEGORY, gg.GOAL_PROGRESS, gg.IMPORTANCE, gp.START_DAY "
        "FROM grown_planner gp "
        "JOIN grown_planner_goal gg ON gp.GROWN_ID = gg.GROWN_ID "
        "WHERE gp.U_ID = '" << uid << "' "
        "  AND gp.GROWN_ID = (SELECT MAX(GROWN_ID) FROM grown_planner WHERE U_ID = '" << uid << "') "
        "ORDER BY gg.IMPORTANCE ASC";

    if (mysql_query(conn_, oss.str().c_str()) != 0) {
        std::cerr << "[DB] fetchLatestGrowPlanner query fail: "
            << mysql_error(conn_) << std::endl;
        return false;
    }

    MYSQL_RES* res = mysql_store_result(conn_);
    if (!res) {
        std::cerr << "[DB] fetchLatestGrowPlanner store_result fail: "
            << mysql_error(conn_) << std::endl;
        return false;
    }

    bool any = false;
    MYSQL_ROW row;
    // 0:GROWN_ID, 1:WANT_JOB, 2:PERIOD, 3:GOAL, 4:GOAL_DATE, 5:CATEGORY, 6:GOAL_PROGRESS, 7:IMPORTANCE
    while ((row = mysql_fetch_row(res)) != nullptr) {
        any = true;

        // 첫 행에서 헤더 채움
        if (outHeader.grown_id == 0) {
            outHeader.grown_id = row[0] ? std::stoi(row[0]) : 0;
            outHeader.want_job = row[1] ? row[1] : "";
            outHeader.period = row[2] ? std::stoi(row[2]) : 0;
        }

        GrowPlannerGoal g;
        g.goal = row[3] ? row[3] : "";
        g.goal_date = row[4] ? row[4] : "";   // NULL이면 ""
        g.category = row[5] ? row[5] : "";
        g.goal_progress = row[6] ? std::stoi(row[6]) : 0;
        g.importance = row[7] ? std::stoi(row[7]) : 0;
        g.start_date = row[8] ? row[8] : "";
        outGoals.push_back(std::move(g));
    }

    mysql_free_result(res);
    return any; // 하나라도 있으면 true
}


//// ------------- UPDATE: 목표 달성 처리 -------------
//bool DBClass::updateGrownGoalAchieved(unsigned long long id,
//    unsigned long long grown_id,
//    const std::string& goal,
//    const std::string& goal_date) {
//    // ⚠ 테이블/컬럼명은 실제 스키마로 맞춰 변경
//    std::string q =
//        "UPDATE GROWN_PLANNER_GOAL"
//        "SET STATUS='DONE', GOAL_DATE='" + escape(conn_, goal_date) + "' "
//        "WHERE ID=" + std::to_string(id) +
//        " AND GROWN_ID=" + std::to_string(grown_id) +
//        " AND GOAL='" + escape(conn_, goal) + "';";
//
//    if (mysql_query(conn_, q.c_str())) {
//        std::cerr << "[DB] UPDATE 실패: " << mysql_error(conn_) << std::endl;
//        return false;
//    }
//    return (mysql_affected_rows(conn_) > 0);
//}

// 목표달성처리
// 목표 달성 일괄 처리
bool DBClass::updateGoalsAchieved(
    const std::string& u_id,
    unsigned long long grown_id,
    const std::vector<std::string>& goals,
    const std::string& date,
    unsigned long& affected
) {
    affected = 0;
    if (!conn_) return false;
    if (goals.empty()) return true; // 업데이트할 게 없으면 성공 취급(영향 0건)

    // IN ('g1','g2',...) 목록 구성 (각 항목 이스케이프)
    std::ostringstream inList;
    inList << "(";
    for (size_t i = 0; i < goals.size(); ++i) {
        if (i) inList << ",";
        inList << "'" << escape(conn_, goals[i]) << "'";
    }
    inList << ")";

    const std::string d = escape(conn_, date);

    std::ostringstream oss;
    oss << "UPDATE " << TBL_GOAL
        << " SET GOAL_DATE = '" << d << "'"
        << " WHERE GROWN_ID = " << grown_id
        << " AND GOAL IN " << inList.str();

    if (mysql_query(conn_, oss.str().c_str()) != 0) {
        std::cerr << "[DB] updateGoalsAchieved query fail: " << mysql_error(conn_) << std::endl;
        return false;
    }

    affected = static_cast<unsigned long>(mysql_affected_rows(conn_));
    return true;
}


// ========== 마이페이지: 프로필 1건 조회 ==========
bool DBClass::getUserInfoById(const std::string& u_id,
    std::string& u_name,
    std::string& u_address,
    std::string& u_phone)
{
    if (!conn_) return false;

    const std::string id_esc = escape(conn_, u_id);
    const std::string q =
        "SELECT U_NAME, U_ADDRESS, U_PHONE "
        "FROM USER_INFO "
        "WHERE U_ID='" + id_esc + "' "
        "LIMIT 1";

    if (mysql_query(conn_, q.c_str())) {
        std::cerr << "[DB] getUserInfoById 쿼리 실패: " << mysql_error(conn_) << std::endl;
        return false;
    }

    MYSQL_RES* res = mysql_store_result(conn_);
    if (!res) {
        std::cerr << "[DB] getUserInfoById 결과 없음/에러: " << mysql_error(conn_) << std::endl;
        return false;
    }

    MYSQL_ROW row = mysql_fetch_row(res);
    bool ok = false;
    if (row) {
        // 컬럼: 0=U_NAME, 1=U_ADDRESS, 2=U_PHONE
        u_name = row[0] ? row[0] : "";
        u_address = row[1] ? row[1] : "";
        u_phone = row[2] ? row[2] : "";
        ok = true;
    }

    mysql_free_result(res);
    return ok;
}

// ========== 마이페이지: 최신 검사결과 1건 조회 ==========
bool DBClass::getLatestResultByUser(const std::string& u_id,
    unsigned int& res_id,
    std::string& res_con)
{
    if (!conn_) return false;

    const std::string id_esc = escape(conn_, u_id);
    // updated_at 컬럼이 없다면 RES_ID 내림차순이 가장 안전
    const std::string q =
        "SELECT RES_ID, RES_CON "
        "FROM USER_TOTAL_RESULT "
        "WHERE U_ID='" + id_esc + "' "
        "ORDER BY RES_ID DESC "
        "LIMIT 1";

    if (mysql_query(conn_, q.c_str())) {
        std::cerr << "[DB] getLatestResultByUser 쿼리 실패: " << mysql_error(conn_) << std::endl;
        return false;
    }

    MYSQL_RES* res = mysql_store_result(conn_);
    if (!res) {
        std::cerr << "[DB] getLatestResultByUser 결과 없음/에러: " << mysql_error(conn_) << std::endl;
        return false;
    }

    MYSQL_ROW row = mysql_fetch_row(res);
    bool ok = false;
    if (row) {
        // ★ 추가: 길이 기반 복사로 TEXT 안전 수신 (내부 따옴표/개행 포함해도 안전)
        unsigned long* lens = mysql_fetch_lengths(res);
        // 컬럼: 0=RES_ID, 1=RES_CON
        res_id = row[0] ? static_cast<unsigned int>(std::strtoul(row[0], nullptr, 10)) : 0U;
        if (row[1] && lens) { //길이값(lens[1])을 활용해 정확히 복사
            res_con.assign(row[1], lens[1]);
        }
        else {
            res_con.clear();
        }
        ok = true;
    }
    mysql_free_result(res);
    return ok;
}