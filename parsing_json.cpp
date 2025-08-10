#include "parsing_Json.h"
#include "server.h"
#include "db_class.h"
#include <nlohmann/json.hpp>
#include <iostream>
#include <sstream>

using json = nlohmann::json;

//생성자 구현
RequestHandler::RequestHandler(TcpServer* server, DBClass* db): server_(server), db_(db) {
    // Protocol 100 → handleProtocol100 매핑
    handlers[100] = [this](int fd, int py_fd, const json& j) { return handleProtocol100(fd, py_fd, j);};
    handlers[1] = [this](int fd, int py_fd, const json& j) { return handleProtocol1(fd, py_fd, j);};
    handlers[2] = [this](int fd, int py_fd, const json& j) { return handleProtocol2(fd, py_fd, j);};
    handlers[3] = [this](int fd, int py_fd, const json& j) { return handleProtocol3(fd, py_fd, j); };
    handlers[4] = [this](int fd, int py_fd, const json& j) { return handleProtocol4(fd, py_fd, j); };
    handlers[6] = [this](int fd, int py_fd, const json& j) { return handleProtocol6(fd, py_fd, j);};
    handlers[7] = [this](int fd, int py_fd, const json& j) { return handleProtocol7(fd, py_fd, j); };
    handlers[8] = [this](int fd, int py_fd, const nlohmann::json& j) {return handleProtocol8(fd, py_fd, j);};
    
    // 성장플래닛 라우팅
    handlers[5]   = [this](int, int py_fd, const json& j){ return handleProtocol5(j);      };
    handlers[9]   = [this](int, int py_fd, const json& j){ return handleProtocol9(j);      };
    handlers[10]  = [this](int, int py_fd, const json& j){ return handleProtocol10(j);     };
    handlers[1005]= [this](int, int py_fd, const json& j){ return handleProtocol100_5(py_fd, j); };
    handlers[1006]= [this](int, int py_fd, const json& j){ return handleProtocol100_6(py_fd, j); };
}
// ② 새로 추가하는 한-인자 생성자(위임) 뭔말이야 아오
RequestHandler::RequestHandler(TcpServer* server)
    : RequestHandler(server, server ? server->getDB() : nullptr) {
}

// ===== 공통 진입점: JSON 문자열 → 파싱 → 핸들러 호출 =====
std::string RequestHandler::process(int client_fd, int python_fd, const std::string& jsonStr,std::string client_ip)
{
    std::cout << "[RequestHandler] process 호출됨" << std::endl;
    std::cout << "Client FD: " << client_fd << ", Python FD: " << python_fd << std::endl;
    std::cout << "Client IP: " << client_ip << std::endl;
    std::cout << "받은 JSON: " << jsonStr << std::endl;

    // JSON 파싱
    json parsed;
    try {
        parsed = json::parse(jsonStr);
    }
    catch (const std::exception& e) {
        return R"({"PROTOCOL":100_2,"ERROR":"Invalid JSON"})"; // 100_2
    }

    //앞 숫자 100만 추출해서 정수형 protocol_num에 저장
    std::string proto_str = parsed.value("protocol", "");
    int protocol_num = -1; //초기화함. 기본 실패값으로
    if (!proto_str.empty() && proto_str.find('_') != std::string::npos) {
        protocol_num = std::stoi(proto_str.substr(0, proto_str.find('_')));
    }

    // map 기반 핸들러 호출 방식으로 변경
    auto it = handlers.find(protocol_num);
    if (it != handlers.end()) {
        return it->second(client_fd, python_fd, parsed);
    }

    return R"({"protocol":"99","ERROR":"Unknown protocol"})";
}

// Protocol 100 처리 (AI 요청)
std::string RequestHandler::handleProtocol100(int client_fd, int python_fd, const nlohmann::json& j) {
    
    // 1. Python 연결 상태 확인
    if (python_fd == INVALID_SOCKET) {
        return R"({"PROTOCOL":100_0_2,"ERROR":"Python not connected"})"; // 100_2
    }
    // 2. AI 요청 처리
    nlohmann::json sendJson = j;
    //sendJson["protocol"] = "100_0_0"; // 요청 시 항상  100_0
    sendJson["protocol"] = j.value("protocol", "100_0_0");  // ★ 핵심 수정

    // 3. Python에 요청 전송
    std::string pythonResponse = server_->sendToPythonAndReceive(sendJson.dump());

    if (pythonResponse.empty()) {
        return R"({"PROTOCOL":100_0_2,"ERROR":"No response from Python"})"; // 100_2
    }

    // 4. Python 응답 처리
    try {
        json respJson = json::parse(pythonResponse);

        // (디버그) Python 응답 키 확인 — 문제 잡히면 지워도 됨
        std::cerr << "[100] python keys: ";
        for (auto it = respJson.begin(); it != respJson.end(); ++it) std::cerr << it.key() << ' ';
        std::cerr << "\n";

        // =====================[ 추가: 100_5_0 빠른 반환 ]=====================
        // 희망직업 검색은 DB 저장(analysis_summary) 대상이 아님 → 그대로 클라이언트로 전달
        if (j.value("protocol", "") == "100_5_0") {
            // Python이 넣어준 프로토콜을 보정(없거나 다르면 100_5_1로 강제)
            respJson["protocol"] = "100_5_1";
            return respJson.dump();
        }
        // ★ 추가: 100_6_0은 전용 핸들러로 바로 위임 (DB 저장 로직도 그쪽에 있음)
        if (j.value("protocol", "") == "100_6_0") {
            respJson["protocol"] = "100_6_1"; // 보정
            return handleProtocol100_6(python_fd, j);  
        }

        // ======== ★ DB 저장: analysis_summary만 RES_CON에 저장 ========
        {
            RequestHandler::TotalResult r;
            r.u_id = j.value("u_id", j.value("U_ID", j.value("userid", j.value("user_id", j.value("id", std::string{})))));
            r.res_con = respJson.value("analysis_summary", std::string{}); // ★핵심: 요약만 저장
            r.res_alive = "N/A";                                             // 기본값
            r.res_act = "PREPARE";                                         // 기본값(ENUM)
            r.status = true;
       
             //===== [실운영용] UID와 summary 둘 다 필수 =====
              //- 운영 전환 시 아래 블록 주석 해제하고, 위 [TEST 전용] 블록은 주석 처리/삭제하세요.
            if (r.u_id.empty() || r.res_con.empty()) {
                return R"({"PROTOCOL":"100_0_2","ERROR":"missing userid or analysis_summary"})";
            }
            // ① USER_TOTAL_RESULT 저장 + 생성된 RES_ID 획득
            uint64_t res_id = 0;
            if (!server_->insertUserTotalResultWithId(r, res_id)) {
                return R"({"PROTOCOL":"100_0_2","ERROR":"DB insert failed (total_result})";
            }
            // ② Python 응답의 recommended_jobs → USER_RES_JOBS 저장
            std::vector<RequestHandler::RecommendedJob> jobs;

            if (respJson.contains("recommended_jobs") && respJson["recommended_jobs"].is_array()) {
                for (const auto& item : respJson["recommended_jobs"]) {
                    if (!item.is_object()) continue;
                    RequestHandler::RecommendedJob row{};
                    row.job = item.value("job", std::string{});
                    row.description = item.value("description", std::string{});
                    row.reason = item.value("reason", std::string{});
                    if (row.job.empty() && row.description.empty() && row.reason.empty()) continue;
                    jobs.push_back(std::move(row));
                }
            }

            if (!jobs.empty()) {
                if (!server_->insertRecommendedJobs(res_id, jobs)) {
                    return R"({"PROTOCOL":"100_0_2","ERROR":"DB insert failed (user_res_jobs})";
                }
            }
        }
        respJson["protocol"] = "100_0_1";
        return respJson.dump();
    }
    catch (const std::exception& e) {
        return R"({"PROTOCOL":100_0_2","ERROR":"No response from Python"})"; // 100_2
    }
}

// Protocol1처리 (로그인 1_0)
std::string RequestHandler::handleProtocol1(int client_fd, int python_fd, const json& j) {
    // 1. ID/PW 추출
    std::string id = j.value("id", "");
    std::string pw = j.value("pw", ""); // 문자열 그대로 사용

    if (id.empty() || pw.empty()) {
        return R"({"protocol":"1_2","message":"ID or PW missing"})";
    }

    // 2. DB에 ID/PW가 존재하는지 확인
    bool loginSuccess = server_->checkLoginFromDB(id, pw);

    json respJson;

    if (loginSuccess) {
        // 3-1. 로그인 성공
        respJson["protocol"] = "1_1";
        respJson["message"] = "Login Success";
    }
    else {
        // 3-2. 로그인 실패 → DB에 해당 ID가 없다고 판단하고 새로 INSERT 시도
        //bool inserted = server_->insertUserToDB(u);
        respJson["protocol"] = "1_2";
        respJson["message"] = "Login failed and user creation failed";

        //if (inserted) {
        //    // 새 유저 저장 성공
        //    respJson["protocol"] = "1_1";
        //    respJson["message"] = "New user registered";
        //}
        //else {
        //    // 저장 실패 (예: 중복, DB 오류 등)
        //    respJson["protocol"] = "1_2";
        //    respJson["message"] = "Login failed and user creation failed";
        //}
    }

    return respJson.dump();
}

// Protocol2처리 (얼굴로그인 2_0)
std::string RequestHandler::handleProtocol2(int client_fd, int python_fd, const json& j) {
    // 1. 얼굴 벡터 존재 여부 확인
    if (!j.contains("face_id") || !j["face_id"].contains("embedding")) {
        return R"({"protocol":"2_2","message":"face_id.embedding missing"})";
    }

    std::vector<float> faceVector;
    try {
        for (const auto& val : j["face_id"]["embedding"]) {
            faceVector.push_back(val.get<float>());
        }
    }
    catch (...) {
        return R"({"protocol":"2_2","message":"Invalid face vector format"})";
    }

    // 2. DB에서 유사 사용자 검색
    std::string matchedUserId = server_->findUserByFaceVector(faceVector);

    // 3. 응답 구성
    json resp;
    if (!matchedUserId.empty()) {
        resp["protocol"] = "2_1";  // 성공
        resp["message"] = "Face login success";
        resp["u_id"] = matchedUserId;
    }
    else {
        resp["protocol"] = "2_2";  // 실패
        resp["message"] = "Face not recognized";
    }

    return resp.dump();
}

// Protocol3처리 (ID중복확인 3_0)
std::string RequestHandler::handleProtocol3(int client_fd, int py_fd, const json& j) {
    
    const std::string userid = j.value("userid", "");
    std::cout << "[3_0] ID 중복 확인 요청 - ID: " << userid << std::endl;

    json response;

    // DB 접근 (둘 중 하나를 사용하세요)
    // 1) TcpServer에 db_ 멤버가 public/accessible한 경우:
    const bool exists = server_->db.isUserIdExists(userid);

    // 2) 또는 TcpServer에 중계 메서드를 만들어 놨다면:
    // const bool exists = server_->isUserIdExists(userid);

    if (!exists) {
        response["protocol"] = "3_1"; // 사용 가능
        response["message"] = "사용 가능한 아이디입니다.";
    }
    else {
        response["protocol"] = "3_2"; // 중복
        response["message"] = "이미 존재하는 아이디입니다.";
    }

    return response.dump(); // 🔸 여기서 문자열만 반환, sendAll은 하지 않음
}

// Protocol4처리 (회원가입 4_0)
std::string RequestHandler::handleProtocol4(int client_fd, int py_fd, const json& j) {
    std::cout << "핸들프로토콜4" << std::endl;

    Signup u;
    u.id = j.value("userid", "");
    u.pw = j.value("pw", "");
    u.name = j.value("name", "");
    u.birth = j.value("birth", "");
    // ★ gender: 숫자로 오므로 안전하게 처리
    if (j.contains("gender") && j["gender"].is_number_integer()) {
        u.gender = std::to_string(j["gender"].get<int>());  // 구조체가 string이면 문자열로 변환
    }
    else {
        u.gender = "";  // 또는 "0"
    }
    u.address = j.value("address", "");
    u.phone = j.value("phone", "");
    // ★ faceembedding: null/array 구분해서 파싱
    u.face.clear();
    if (j.contains("faceembedding") && j["faceembedding"].is_array()) {
        try {
            u.face = j["faceembedding"].get<std::vector<float>>();
        }
        catch (...) {
            u.face.clear();
        }
    }
    //u.gender = j.value("gender", "");
    //u.address = j.value("address", "");
    //u.phone = j.value("phone", "");
    //try { if (j.contains("faceembedding")) u.face = j.at("faceembedding").get<std::vector<float>>(); }
    //catch (...) { u.face.clear(); }
    json resp;

    if (u.id.empty() || u.pw.empty()) {
        resp["protocol"] = "4_2";
        resp["message"] = "필수 항목 누락(userid/pw)";
        std::cout << "뭘찍어?" << std::endl;
        return resp.dump();
    }

    if (server_->isUserIdExists(u.id)) {             // ✅ 중계 메서드
        resp["protocol"] = "4_2";
        resp["message"] = "이미 존재하는 아이디입니다.";
        return resp.dump();
    }

    if (server_->insertUserToDB(u)) {         // ✅ 중계 메서드
        resp["protocol"] = "4_1";
        resp["message"] = "회원가입이 완료되었습니다.";
        std::cout << "성공값 들어왔나요" << std::endl;
        
    }
    else {
        resp["protocol"] = "4_2";
        resp["message"] = "회원가입에 실패했습니다.";
        std::cout << "실패값 들어왔나요" << std::endl;

    }
    return resp.dump();
}

// Protocol5처리 (플래닛 5_0 최신추천직업 6개 가져오기)
std::string RequestHandler::handleProtocol5(const json& j) {
    const std::string u_id = j.value("u_id", "");
    if (u_id.empty()) {
        return json{ {"protocol","5_2"},{"error","u_id is required"} }.dump();
    }
    std::cout << "프로토콜 1" << endl;

    // 1) 캐시 히트 & 유효 검사
    std::vector<RequestHandler::RecommendedJob> items;
    //unsigned int res_id = 0;
    string res_id;
    {
        std::lock_guard<std::mutex> lk(latest_jobs_mtx_);
        auto it = latest_jobs_cache_.find(u_id);
        if (it != latest_jobs_cache_.end()) {
            const auto now = std::chrono::steady_clock::now();
            const auto age_min =
                std::chrono::duration_cast<std::chrono::minutes>(now - it->second.saved_at).count();
            if (age_min <= JOBS_CACHE_TTL_MIN && !it->second.items.empty()) {
                res_id = it->second.res_id;
                items = it->second.items;
            }
        }
    }

    // 2) 캐시 미스/만료 → SP 호출
    if (items.empty()) {
        if (!db_) return json{ {"protocol","5_2"},{"error","db not ready"} }.dump();     // ★ db_ 로 수정

        std::string raw;
        if (!db_->getUserJobsLatestRes(u_id, raw)) {                                   // ★ db_ 로 수정
            return json{ {"protocol","5_2"},{"error","db call failed"} }.dump();
        }
        //std::cerr << "[DEBUG] raw from SP: " << raw << "\n";

        json sp = json::parse(raw, nullptr, false);
        if (sp.is_discarded()) {
            return json{ {"protocol","5_2"},{"error","invalid json from sp"} }.dump();
        }

        res_id = sp.value("RES_ID", "null");

        if (sp.contains("items") && sp["items"].is_array()) {
            for (auto& row : sp["items"]) {                                            // 변수명 혼동 방지
                RecommendedJob rj;
                rj.job = row.value("JOB", "");
                rj.description = row.value("JOB_EXPLAIN", "");
                rj.reason = row.value("JOB_REASON", "");
                if (!rj.job.empty()) items.push_back(std::move(rj));
            }
        }

        if (!items.empty()) {
            std::lock_guard<std::mutex> lk(latest_jobs_mtx_);
            //latest_jobs_cache_[u_id] = JobsCache{
            //    res_id, items, std::chrono::steady_clock::now()
            //};
        }
    }

    // 3) 6개 제한
    if (items.size() > 6) items.resize(6);

    // 4) 응답 JSON
    json outItems = json::array();
    for (const auto& rj : items) {
        outItems.push_back({
            {"job",         rj.job},
            {"reason",      rj.reason},
            {"description", rj.description}
            });
    }

    return json{
        {"protocol","5_1"},
        {"u_id",u_id},
        {"res_id",res_id},
        {"items",outItems}
    }.dump();
}

// Protocol100_5_0처리 (희망직업 검색/유추 100_5_0 )
std::string RequestHandler::handleProtocol100_5(int python_fd, const json& j) {
    if (python_fd == INVALID_SOCKET) {
        return json{ {"protocol","100_5_2"},{"error","Python not connected"} }.dump();
    }

    const std::string u_id = j.value("u_id", "");
    const std::string query = j.value("query", "");
    if (u_id.empty() || query.empty()) {
        return json{ {"protocol","100_5_2"},{"error","u_id and query are required"} }.dump();
    }

    // Python으로 보낼 페이로드
    json pyReq{
        {"protocol","PY_JOB_SEARCH"},
        {"u_id",    u_id},
        {"query",   query},
        {"topk",    3},
        {"lang",    "ko"}
    };

    // 길이+본문 송수신은 TcpServer::sendToPythonAndReceive가 처리
    std::string pyRaw = server_->sendToPythonAndReceive(pyReq.dump());
    if (pyRaw.empty()) {
        return json{ {"protocol","100_5_2"},{"error","no response from python"} }.dump();
    }

    json py = json::parse(pyRaw, nullptr, false);
    if (py.is_discarded()) {
        return json{ {"protocol","100_5_2"},{"error","invalid python json"} }.dump();
    }

    // 그대로 통과시켜도 되지만, 안전하게 필요한 필드만 유지
    json search;
    search["exact"] = py.value("exact", false);
    if (search["exact"].get<bool>()) {
        search["job"] = py.value("job", "");
    }
    else {
        search["candidates"] = py.value("candidates", json::array());
    }

    return json{
        {"protocol","100_5_1"},
        {"u_id",u_id},
        {"search",search}
    }.dump();
}

// Protocol100_6_0처리 (선택한 직업으로 플래닛 생성 100_6_0 )
std::string RequestHandler::handleProtocol100_6(int python_fd, const json& j) {
    if (python_fd == INVALID_SOCKET) {
        return json{ {"protocol","100_6_2"},{"error","Python not connected"} }.dump();
    }

    const std::string u_id = j.value("u_id", "");
    const std::string job = j.value("job", "");
    if (u_id.empty() || job.empty()) {
        return json{ {"protocol","100_6_2"},{"error","u_id and job are required"} }.dump();
    }

    // 1) Python에게 선택 직업 전달 → 정규화/유효성 검사/목표 제안 등
    json pyReq{
        {"protocol","PY_GROWN_PLAN_BUILD"},
        {"u_id",    u_id},
        {"job",     job}
    };
    std::string pyRaw = server_->sendToPythonAndReceive(pyReq.dump());
    if (pyRaw.empty()) {
        return json{ {"protocol","100_6_2"},{"error","no response from python"} }.dump();
    }
    json py = json::parse(pyRaw, nullptr, false);
    if (py.is_discarded()) {
        return json{ {"protocol","100_6_2"},{"error","invalid python json"} }.dump();
    }

    // ------------------------------------------------------------
    // [추가] Python이 DB 저장용 파라미터(params)를 줬다면: 신규 SP(4인자) 경로 사용
    //  - params: { "period": 정수, "goal_json": [ ... ] }
    //  - 성공 시 raw 에 간단한 OK JSON을 넣어 아래 기존 흐름(sp 파싱)을 유지
    // ------------------------------------------------------------
    std::string raw; // ← 기존 변수 유지
    std::string db_err;         // ★ 추가: 에러 메시지 받을 곳
    std::string out_json;       // ★ 추가: SP 5-인자형의 in/out 버퍼
    int period_for_sp = 0;      // ★ 추가: 기본 0 (레거시 경로에선 무시됨)

    if (py.contains("params") && py["params"].is_object()) {
        const auto& params = py["params"];
        const int period = params.value("period", 180);
        std::string out_json = params.contains("out_json") ? params["out_json"].dump() : "[]";

        if (!db_) {
            return json{ {"protocol","100_6_2"},{"error","db not ready"} }.dump();
        }

        if (!db_ || !db_->insertGrownPlannerWithGoals(u_id, period_for_sp, job, out_json, db_err)) {
            return json{ {"protocol","100_6_2"},{"error", std::string("db call failed: ") + db_err} }.dump();
        }
        raw = out_json; //함수 내부에서 out_json이 OK JSON 등으로 갱신됨
    }
    // ------------------------------------------------------------


    // 2) Python이 OK를 주면 DB SP로 최종 저장(필요 시 py 내용 사용 가능)
    //    여기서는 job만 넘기는 기존 SP 시그니처에 맞춰 호출
    if (raw.empty()) {
        // 현재 헤더에 3-인자 오버로드가 없으므로 5-인자 형태로 레거시 경로 트리거
        std::string out_json2;    // 비워두면 내부에서 레거시 SP(@p_result) 경로 실행
        std::string db_err2;
        int period_dummy = 0;
        if (!db_ || !db_->insertGrownPlannerWithGoals(u_id, period_dummy, job, out_json2, db_err2)) {
            return json{ {"protocol","100_6_2"},{"error", std::string("db call failed: ") + db_err2} }.dump();
        }
        raw = out_json2;
    }
    json sp = json::parse(raw, nullptr, false);
    if (sp.is_discarded()) {
        return json{ {"protocol","100_6_2"},{"error","invalid json from sp"} }.dump();
    }
    return json{
        {"protocol","100_6_1"},
        {"u_id",u_id},
        {"result",sp}
    }.dump();
}

// Protocol6처리 (이력서관리 조회 6_0 , 최신 1건)
std::string RequestHandler::handleProtocol6(int client_fd, int python_fd, const json& j) {
    (void)client_fd; (void)python_fd;

    // u_id 파싱: 클라가 u_id 또는 userid로 보낼 수 있음
    std::string u_id;
    if (j.contains("u_id") && j["u_id"].is_string())        u_id = j["u_id"].get<std::string>();
    else if (j.contains("userid") && j["userid"].is_string()) u_id = j["userid"].get<std::string>();

    if (u_id.empty()) {
        return R"({"protocol":"6_2","error":"u_id required"})";
    }

    // 최신 1건 조회
    ResumeRow row; // db_class.h에 정의되어 있어야 함
    // server_->db 는 객체입니다(포인터 아님). 래퍼가 있으면 server_->getUserCVByUid(...) 사용 가능
    if (!server_->db.getUserCVByUid(u_id, row)) {
        return R"({"protocol":"6_2","error":"resume not found"})";
    }

    // CV_SIGNI를 notes + license 배열로 분리 (첫 줄은 notes, 이후 줄을 license로 가정)
    std::string notes;
    std::vector<std::string> licenses;
    {
        std::istringstream iss(row.signi);
        std::string line;
        bool first = true;
        while (std::getline(iss, line)) {
            if (first) { notes = line; first = false; }
            else if (!line.empty()) licenses.push_back(line);
        }
    }

    // 응답 구성
    json resp{
        {"protocol","6_1"},
        {"cv_id",   row.cv_id},
        {"u_id",    row.u_id},
        {"name",    row.name},
        {"birth",   row.birth},     // "YYYY-MM-DD"
        {"email",   row.email},
        {"phone",   row.phone},
        {"address", row.address},
        {"career",  row.career},
        {"notes",   notes},
        {"license", licenses}       // 없으면 []
    };
    return resp.dump();
}

// Protocol7처리 (이력서관리 저장 7_0)
std::string RequestHandler::handleProtocol7(int client_fd, int python_fd, const json& j) {
    (void)client_fd; (void)python_fd;

    // -------- 유틸: 여러 키 중 처음 발견되는 값을 고르는 헬퍼 --------
    auto pick = [&](std::initializer_list<const char*> keys) -> std::string {
        for (auto k : keys) {
            auto it = j.find(k);
            if (it != j.end() && it->is_string()) return it->get<std::string>();
        }
        return {};
        };

    // -------- 값 파싱 (클라/서버 혼용 키 모두 허용) --------
    std::string u_id = pick({ "u_id", "userid", "U_ID" });
    std::string name = pick({ "cv_name", "name" });
    std::string birth = pick({ "cv_birth", "birth", "birthdate" }); // YYYY-MM-DD
    std::string email = pick({ "cv_email", "email" });
    std::string phone = pick({ "cv_phone", "phone" });
    std::string address = pick({ "cv_address", "address" });
    std::string career = pick({ "cv_career", "career" });
    std::string notes = pick({ "cv_signi", "notes" });

    // 전화번호 정규화: 하이픈/공백 제거 (DB는 VARCHAR(20)이라 안전)
    {
        std::string cleaned;
        cleaned.reserve(phone.size());
        for (unsigned char c : phone) if (std::isdigit(c)) cleaned.push_back(c);
        if (!cleaned.empty()) phone = cleaned;
    }

    // license 배열을 CV_SIGNI 뒤에 줄바꿈으로 붙임 (테이블에 별도 컬럼이 없기 때문)
    if (j.contains("license") && j["license"].is_array()) {
        for (const auto& it : j["license"]) {
            if (it.is_string()) {
                if (!notes.empty()) notes += "\n";
                notes += it.get<std::string>();
            }
        }
    }

    // -------- 필수값 검증: 누락 항목을 구체적으로 반환 --------
    std::vector<std::string> missing;
    if (u_id.empty())    missing.push_back("u_id");
    if (name.empty())    missing.push_back("name/cv_name");
    if (birth.empty())   missing.push_back("birth(YYYY-MM-DD)");
    if (email.empty())   missing.push_back("email");
    if (phone.empty())   missing.push_back("phone");
    if (address.empty()) missing.push_back("address");

    if (!missing.empty()) {
        json resp{
            {"protocol", "7_2"},
            {"error", "missing required fields"},
            {"missing", missing}
        };
        return resp.dump();
    }

    // -------- DB 저장 (없으면 INSERT, 있으면 UPDATE) --------
    RequestHandler::Resume cv{ u_id, name, email, phone, birth, address, career, notes };
    uint64_t cv_id = 0;
    bool ok = server_->upsertUserCV(cv, cv_id);

    json resp;
    if (ok) {
        resp["protocol"] = "7_1";
        resp["cv_id"] = cv_id;   // 생성/수정된 CV_ID
    }
    else {
        resp["protocol"] = "7_2";
        resp["error"] = "db error";
    }
    return resp.dump();
}

// Protocol8처리 (마이페이지 8_0)
std::string RequestHandler::handleProtocol8(int client_fd, int /*python_fd*/, const json& j)
{
    // 1) 입력 파싱 (u_id는 소문자/대문자 모두 지원)
    const std::string u_id = j.value("U_ID", j.value("u_id", std::string{}));

    json resp;
    resp["protocol"] = "8_1";

    // 기본값: 데이터 없음(null)로 채워서 클라이언트가 분기 처리 가능하도록 함
    resp["user_info"] = nullptr;
    resp["latest_result"] = nullptr;

    if (u_id.empty()) {
        // u_id가 없으면 빈 응답(두 필드 null) 반환
        return resp.dump();
    }

    // 2) DB 조회
    std::string u_name, u_address, u_phone;
    bool hasUser = false;

    unsigned int res_id = 0;
    std::string res_con;
    bool hasResult = false;

    if (db_) {
        hasUser = db_->getUserInfoById(u_id, u_name, u_address, u_phone);
        hasResult = db_->getLatestResultByUser(u_id, res_id, res_con);
    }
    else {
        std::cerr << "[8] DB 인스턴스가 없습니다.\n";
    }

    // 3) JSON 구성
    if (hasUser) {
        json user_info;
        user_info["U_ID"] = u_id;
        user_info["U_NAME"] = u_name;
        user_info["U_ADDRESS"] = u_address;
        user_info["U_PHONE"] = u_phone;
        resp["user_info"] = user_info;
    }

    if (hasResult) {
        json latest_result;
        latest_result["RES_ID"] = res_id;
        latest_result["U_ID"] = u_id;
        latest_result["RES_CON"] = res_con;
        resp["latest_result"] = latest_result;
    }

    // 4) 직렬화 후 반환 (상위 디스패처에서 송신)
    return resp.dump();
}

// Protocol9_0처리 (최신 플래닛 정보 조회 9_0 )
std::string RequestHandler::handleProtocol9(const json& j) {
    const std::string u_id = j.value("u_id", "");
    if (u_id.empty()) {
        return json{ {"protocol","9_2"},{"error","u_id is required"} }.dump();
    }

    std::string raw;
    if (!db_ || !db_->getGrownPlannerWithGoalsLatest(u_id, raw)) {
        return json{ {"protocol","9_2"},{"error","db call failed"} }.dump();
    }

    json sp = json::parse(raw, nullptr, false);
    if (sp.is_discarded()) {
        return json{ {"protocol","9_2"},{"error","invalid json from sp"} }.dump();
    }

    return json{
        {"protocol","9_1"},
        {"u_id",u_id},
        {"result",sp}
    }.dump();
}

// Protocol10_0처리 (목표 달성 처리 10_0 )
std::string RequestHandler::handleProtocol10(const json& j) {
    unsigned long long id = j.value("id", 0ULL);
    unsigned long long grown_id = j.value("GROWN_ID", 0ULL);
    const std::string  goal = j.value("GOAL", "");
    const std::string  date = j.value("GOAL_DATE", "");

    if (!id || !grown_id || goal.empty() || date.empty()) {
        return json{ {"protocol","10_2"},{"error","id, GROWN_ID, GOAL, GOAL_DATE required"} }.dump();
    }

    if (!db_ || !db_->updateGrownGoalAchieved(id, grown_id, goal, date)) {
        return json{ {"protocol","10_2"},{"error","update failed"} }.dump();
    }

    return json{
        {"protocol","10_1"},
        {"id",id},
        {"GROWN_ID",grown_id},
        {"GOAL",goal},
        {"GOAL_DATE",date}
    }.dump();
}

// 추천직업 캐시 갱신 : 결과를 메모리 캐시에 저장해 두려고 호출. DB를 다시 안 치고 바로 캐시에서 꺼내 응답
void RequestHandler::updateJobsCacheFromRecommended(const std::string& u_id,
    uint64_t res_id,
    const std::vector<RecommendedJob>& items) {
    std::lock_guard<std::mutex> lk(latest_jobs_mtx_);
    latest_jobs_cache_[u_id] = JobsCache{ res_id, items, std::chrono::steady_clock::now() };
}