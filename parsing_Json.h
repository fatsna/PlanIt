#pragma once
#include <functional>
#include <unordered_map>
#include <iostream>
#include <fstream>
// Windows 전용
#include <winsock2.h>   // socket(), bind(), listen(), accept(), recv(), send()
#include <ws2tcpip.h>   // inet_ntop(), inet_pton(), IP 변환
#pragma comment(lib, "ws2_32.lib") // Winsock 라이브러리 링크
#include <cstring> // C 문자열 처리
#include <fstream> //파일 입출력
#include <nlohmann/json.hpp>
#include <string>
#include <vector>
#include <mutex>
#include <chrono>
#include <unordered_map>
#include <cstdint>

using json = nlohmann::json;
using namespace std;
class DBClass;
class TcpServer; // TcpServer 클래스 전방 선언 (헤더 간 순환 참조 방지)



struct ResumeRow {
    uint64_t cv_id{};
    std::string u_id, name, email, phone, birth, address, career, signi;
};

class RequestHandler {
public:

    // 기존 코드와 호환용: TcpServer* 하나만 받는 생성자
    explicit RequestHandler(TcpServer* server);
    RequestHandler(TcpServer* server, DBClass* db); // 생성자에서 TcpServer 포인터 받기

    std::string process(int client_fd,int python_fd,const std::string& jsonStr,std::string client_ip);
    
    // 추천직업 1행 구조
    struct RecommendedJob {
        std::string job;
        std::string description;
        std::string reason;
    };

    //회원가입 구조체
    struct Signup {
        std::string id;
        std::string pw;
        std::string name;
        std::string birth;   // "YYYY-MM-DD"
        std::string gender;  // "M"/"F" 등
        std::string address;
        std::string phone;
        std::vector<float> face; // 512차원 임베딩
    };

    //종합 결과 저장용 페이로드
    struct TotalResult {
        std::string u_id;       // USER_INFO.U_ID
        std::string res_con;    // Python 응답 원문(JSON 문자열) -> TEXT
        std::string res_alive;  // 생존기간 텍스트(없으면 "N/A")
        std::string res_act;    // ENUM: "QUIT_NOW" | "PREPARE" | "TAKE_BREAK"
        bool        status;     // true/false -> 1/0
    };


    //// 이력서 조회 (6_0)
    //struct ResumeRow {
    //    uint64_t cv_id{};
    //    std::string u_id, name, email, phone, birth, address, career, signi;
    //};


    // 이력서 저장 (7_0 )
    struct Resume {
        std::string u_id;      // USER_INFO.U_ID (FK)
        std::string name;      // CV_NAME
        std::string email;     // CV_EMAIL
        std::string phone;     // CV_PHONE
        std::string birth;     // CV_BIRTH  -> "YYYY-MM-DD"
        std::string address;   // CV_ADDRESS
        std::string career;    // CV_CAREER
        std::string signi;     // CV_SIGNI  -> 메모/자격증 등 텍스트
    };


    // 5_0 / 5_1 / 5_2 : (DB) 플래닛 시작하기 - 최신 추천직업 6개
    std::string handleProtocol5(const nlohmann::json& j);

    // 100_5_0 / 100_5_1 / 100_5_2 : (Python) 희망직업 검색/유추
    std::string handleProtocol100_5(int python_fd, const nlohmann::json& j);

    // 100_6_0 / 100_6_1 / 100_6_2 : (Python → DB) 선택한 직업으로 플래닛/목표 생성
    std::string handleProtocol100_6(int python_fd, const nlohmann::json& j);

    // 9_0 / 9_1 / 9_2 : (DB) 최신 플래닛 정보 받기
    std::string handleProtocol9(const nlohmann::json& j);

    // 10_0 / 10_1 / 10_2 : (DB) 목표 달성 처리
    std::string handleProtocol10(const nlohmann::json& j);

    // (선택) 100_0 종합분석 시 캐시 갱신용 헬퍼 — 니 코드에 이미 있으면 생략 가능
    void updateJobsCacheFromRecommended(const std::string& u_id, uint64_t res_id, const std::vector<RecommendedJob>& items);


private:

    //TcpServer* server_; ///< TCP 서버 인스턴스 포인터
    DBClass* db_{ nullptr };
    TcpServer* server_{ nullptr };
    
    using HandlerFunc = std::function<string(int, int, const nlohmann::json&)>; // 프로토콜 번호에 따라 실행 할 함수 저장하는곳
    std::unordered_map<int, HandlerFunc> handlers; //Key-Value 구조를 가진 해시맵

    
    // 추천직업 캐시 구조 (u_id 별)
    struct JobsCache {
        uint64_t res_id{ 0 };
        std::vector<RecommendedJob> items;
        std::chrono::steady_clock::time_point saved_at;
    };
    // ---------- 추천직업 캐시 ----------
    std::unordered_map<std::string, JobsCache> latest_jobs_cache_;
    std::mutex latest_jobs_mtx_;
    // 캐시 TTL(분) — 필요시 조정
    static constexpr long long JOBS_CACHE_TTL_MIN = 60;


    // ===== 프로토콜별 처리 함수 선언 ====
    std::string handleProtocol100(int client_fd, int python_fd, const nlohmann::json& j);
    // 신규 로그인 프로토콜 처리
    std::string handleProtocol1(int client_fd, int python_fd, const nlohmann::json& j);
    // 얼굴로그인 처리
    std::string handleProtocol2(int client_fd, int python_fd, const nlohmann::json& j);
    // ID중복확인 (3_0)
    std::string handleProtocol3(int client_fd, int python_fd, const nlohmann::json& j);
    // 회원가입 (4_0)
    std::string handleProtocol4(int client_fd, int python_fd, const nlohmann::json& j);
    // 이력서 조회 (6_0)
    std::string handleProtocol6(int client_fd, int python_fd, const json& j);
    // 이력서 저장(7_0)
    std::string handleProtocol7(int client_fd, int python_fd, const nlohmann::json& j);
    // 마이페이지 조회 (8_0)
    std::string handleProtocol8(int client_fd, int python_fd, const nlohmann::json& j);


    // ===== 성장플래닛 프로토콜 선언 =====
    // (5_0/5_1/5_2) 플래닛 시작하기: 최신 추천직업 6개
    std::string handleProtocol5(int client_fd, int python_fd, const nlohmann::json& j);
    // (100_5_0/100_5_1/100_5_2) 직업 검색(사용자 입력 → Python 유추)
    std::string handleProtocol100_5(int client_fd, int python_fd, const nlohmann::json& j);
    // (100_6_0/100_6_1/100_6_2) 선택 직업으로 플래닛 만들기 (Python → DB)
    std::string handleProtocol100_6(int client_fd, int python_fd, const nlohmann::json& j);
    // (9_0/9_1/9_2) 최신 플래닛 정보 받기
    std::string handleProtocol9(int client_fd, int python_fd, const nlohmann::json& j);
    // (10_0/10_1/10_2) 목표 달성 처리
    std::string handleProtocol10(int client_fd, int python_fd, const nlohmann::json& j);

};