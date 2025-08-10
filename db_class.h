
#pragma once
#include <string>
#include <mysql.h>
#include <iostream>
#include <vector>
#include <cstring>
#include <cstdlib>
#include <nlohmann/json.hpp>
#include "parsing_Json.h"

//// 조회 결과(6_0 → 6_1)
//struct ResumeRow {                 
//    uint64_t cv_id{};
//    std::string u_id, name, email, phone, birth, address, career, signi;
//};


class DBClass {
public:
    DBClass();
    ~DBClass();

    bool connectDB(const std::string& host,
        const std::string& user,
        const std::string& password,
        const std::string& database,
        unsigned int port = 3306);

    void closeDB();


    // 로그인확인
    bool checkLoginFromDB(const std::string& id, const std::string& pw);
    // 얼굴로그인 검증
    std::string findUserByFace(const std::vector<float>& faceVec);
    // ID중복 확인
    bool isUserIdExists(const std::string& id);
    // 회원가입
    bool insertUserToDB(const RequestHandler::Signup& u);
    // 종합분석결과 저장
    bool insertUserTotalResult(const RequestHandler::TotalResult& r);
    // USER_TOTAL_RESULT 저장 + 생성된 RES_ID를 out으로 받는 버전
    bool insertUserTotalResultWithId(const RequestHandler::TotalResult& r, uint64_t& out_res_id);
    // 추천 직업들 저장 (URJ_ID는 AUTO_INCREMENT 가정 → 명시 안함)
    bool insertRecommendedJobs(uint64_t res_id, const std::vector<RequestHandler::RecommendedJob>& jobs);
    // 이력서관리 (조회 6_0)
    bool getUserCVByUid(const std::string& u_id, ResumeRow& out);
    // 이력서관리 ( 7_0 USER_CV: 있으면 UPDATE, 없으면 INSERT. 최종 CV_ID를 out으로 반환 )
    bool upsertUserCV(const RequestHandler::Resume& cv, uint64_t& out_cv_id);
    // 마이페이지관리 (조회 8_0)
    bool getUserInfoById(const std::string& u_id,std::string& u_name,std::string& u_address,std::string& u_phone);
    bool getLatestResultByUser(const std::string& u_id,unsigned int& res_id,std::string& res_con);
    //[프로시저] 최신 추천직업 6개 묶음(JSON) 가져오기
    bool getUserJobsLatestRes(const std::string& u_id, std::string& out_json);
    // [프로시저] 선택 직업으로 플래닛 생성 + 목표 세팅 (Python 응답 확인 후 호출)
    bool insertGrownPlannerWithGoals(const std::string& u_id, int period, const std::string& job, std::string& out_json, std::string& out_err);
    // [프로시저] 최신 플래닛 + 목표 묶음(JSON) 가져오기
    bool getGrownPlannerWithGoalsLatest(const std::string& u_id, std::string& out_json);
    // [UPDATE] 목표 달성 처리(예시 테이블/컬럼명: 필요시 수정)
    bool updateGrownGoalAchieved(unsigned long long id, unsigned long long grown_id, const std::string& goal,const std::string& goal_date);




private:
    MYSQL* conn_ = nullptr;

    // SQL 인젝션 방지용 escape
    std::string escape(MYSQL* conn, const std::string& s);

    // CALL 이후 남을 수 있는 결과셋/상태 비우기
    void drainResults();
};