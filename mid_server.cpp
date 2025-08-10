#include "server.h"
#include "parsing_Json.h"
#include "db_class.h"
#include <winsock2.h>
#include <ws2tcpip.h>
#include <nlohmann/json.hpp>
#include <iostream>
#include <thread>
#include <mutex>
#include <windows.h>

#pragma comment(lib, "ws2_32.lib")

using namespace std;

// TcpServer의 DB 로그인 체크 함수 구현
bool TcpServer::checkLoginFromDB(const std::string& id, const std::string& pw) {
    return db.checkLoginFromDB(id, pw);
}
// TcpServer의 DB 회원가입 구현
bool TcpServer::insertUserToDB(const RequestHandler::Signup& u) {
    return db.insertUserToDB(u);
}
// TcpServer의 DB 얼굴로그인
std::string TcpServer::findUserByFaceVector(const std::vector<float>& faceVec) {
    return db.findUserByFace(faceVec);  // DBClass 호출
}
// TcpServer의 DB ID 중복 확인
bool TcpServer::isUserIdExists(const std::string& id) {
    return db.isUserIdExists(id);
}
// TcpServer의 DB 종합결과 저장
bool TcpServer::insertUserTotalResult(const RequestHandler::TotalResult& r)
{
    return db.insertUserTotalResult(r);
}
// TcpServer의 DB 종합결과 (추천 & 직업 테이블 1)
bool TcpServer::insertUserTotalResultWithId(const RequestHandler::TotalResult& r, uint64_t& out_res_id) {
    return db.insertUserTotalResultWithId(r, out_res_id);
}
// TcpServer의 DB 종합결과 (추천 & 직업 테이블 2)
bool TcpServer::insertRecommendedJobs(uint64_t res_id,
    const std::vector<RequestHandler::RecommendedJob>& jobs) {
    return db.insertRecommendedJobs(res_id, jobs);
}

// TcpServer의 DB 이력서관리 저장
bool TcpServer::upsertUserCV(const RequestHandler::Resume& cv, uint64_t& out_cv_id) {
    return db.upsertUserCV(cv, out_cv_id);
}



// ============================================================
// TcpServer 생성자 / 소멸자
// ============================================================
TcpServer::TcpServer(int port)
    : port_(port), server_fd_(INVALID_SOCKET), python_fd_(INVALID_SOCKET), running_(false) {
}

TcpServer::~TcpServer() {
    stop();
}

// ============================================================
// Winsock 초기화 및 서버 소켓 생성
// ============================================================
bool TcpServer::createSocket() {
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        cerr << "WSAStartup failed: " << WSAGetLastError() << endl;
        return false;
    }
    server_fd_ = socket(AF_INET, SOCK_STREAM, 0);
    if (server_fd_ == INVALID_SOCKET) {
        cerr << "socket 실패: " << WSAGetLastError() << endl;
        WSACleanup();
        return false;
    }
    return true;
}

// ============================================================
// 서버 소켓 바인딩
// ============================================================
bool TcpServer::bindSocket() {
    sockaddr_in address{};
    address.sin_family = AF_INET;
    address.sin_addr.s_addr = INADDR_ANY;
    address.sin_port = htons(port_);

    int opt = 1;
    setsockopt(server_fd_, SOL_SOCKET, SO_REUSEADDR, (const char*)&opt, sizeof(opt));

    if (::bind(server_fd_, reinterpret_cast<struct sockaddr*>(&address), sizeof(address)) == SOCKET_ERROR) {
        cerr << "bind failed: " << WSAGetLastError() << endl;
        return false;
    }
    return true;
}

// ============================================================
// 서버 소켓 리스닝 시작
// ============================================================
bool TcpServer::listenSocket() {
    return listen(server_fd_, 5) != SOCKET_ERROR;
}

// ============================================================
// 전체 데이터 전송 함수 (길이/본문 포함 전송 보조 함수)
// ============================================================
bool TcpServer::sendAll(SOCKET sock, const char* data, int length) {
    int totalSent = 0;
    while (totalSent < length) {
        int sent = send(sock, data + totalSent, length - totalSent, 0);
        if (sent <= 0) return false; // 오류 or 연결 종료
        totalSent += sent;
    }
    return true;
}

// ============================================================
// 정확한 길이만큼 수신 함수 (길이/본문 포함 수신 보조 함수)
// ============================================================

bool TcpServer::recvAll(SOCKET sock, char* buffer, int length) {
    int totalRecv = 0;
    while (totalRecv < length) {
        int received = recv(sock, buffer + totalRecv, length - totalRecv, 0);
        if (received <= 0) return false; // 오류 or 연결 종료
        totalRecv += received;

        //디버깅용
        std::cout << "[recvAll] 누적 수신 바이트: " << totalRecv
            << "/" << length << " (이번 수신: " << received << ")" << std::endl;
    }
    return true;
}

// ============================================================
// WPF 클라이언트 요청 처리
// (길이 + 본문 구조 유지)
// ============================================================
void TcpServer::handleClient(int client_fd) {
    std::cout << "[C++] 클라이언트 연결됨" << endl;

    //디버깅
    static int msgCount = 0; // 전체 수신 메시지 카운터

    while (true) {
        // 1. 길이(4바이트) 수신
        uint32_t msgLenNet; // 네트워크에서 받은 길이 (Big Endian)
        if (!recvAll(client_fd, (char*)&msgLenNet, sizeof(msgLenNet))) {
            cerr << "[C++] 길이 수신 실패" << endl;
            break;
        }

        uint32_t msgLen = ntohl(msgLenNet); // Host Endian으로 변환

        // 길이 값 유효성 검사
        if (msgLen == 0 || msgLen > 10 * 1024 * 1024) {
            cerr << "[C++] 비정상적인 메시지 길이: " << msgLen << endl;
            break;
        }
        // 2. JSON 본문 수신
        string jsonStr(msgLen, '\0'); // 정확한 크기만큼 문자열 버퍼 생성
        if (!recvAll(client_fd, &jsonStr[0], msgLen)) {
            cerr << "[C++] 본문 수신 실패" << endl;
            break;
        }
        std::cout << "[C++] 받은 JSON: " << jsonStr << endl;

        //디버깅
        msgCount++;
        std::cout << "[C++] (" << msgCount << ") 받은 JSON 일부: "
            << jsonStr.substr(0, 200) << "..." << std::endl;


        // ===== 3. 프로토콜 요청 처리 =====
        RequestHandler handler(this); //this : TcpServer객체의 포인터

        string aiResponse = handler.process( //process가 결과를 문자열로 반환
            client_fd, 
            python_fd_, 
            jsonStr,
            ""
            //getClientIP(client_fd)
        );

        // 4. 응답 길이(4바이트) + 본문 전송
        uint32_t respLenNet = htonl((uint32_t)aiResponse.size());
        if (!sendAll(client_fd, (char*)&respLenNet, sizeof(respLenNet))) {
            cerr << "[C++] 응답 길이 전송 실패" << endl;
            break;
        }
        if (!sendAll(client_fd, aiResponse.c_str(), (int)aiResponse.size())) {
            cerr << "[C++] 응답 데이터 전송 실패" << endl;
            break;
        }
        //std::cout << "[C++] 응답 전송 완료" << endl;

        //디버깅
        std::cout << "[C++] 응답 전송 완료 (길이: " << aiResponse.size() << " bytes)" << std::endl;
        std::cout << "[C++] 응답 전송 완료 (내용: " << aiResponse << std::endl;
      
    }
    closesocket(client_fd);
    std::cout << "[C++] 클라이언트 소켓 종료" << endl;
}

// ============================================================
// Python에 요청 보내고 응답 받기
// (길이 + 본문 구조 유지)
// ============================================================
string TcpServer::sendToPythonAndReceive(const string& jsonStr) {
    if (python_fd_ == INVALID_SOCKET) {
        cerr << "[C++] Python 미연결 상태!" << endl;
        return R"({"PROTOCOL":999,"TEXT":"Python not connected"})";
    }

    // 요청 데이터 준비
    string sendData = jsonStr.empty()
        ? R"({"PROTOCOL":0,"TEXT":"PING TEST"})"
        : jsonStr;

    std::cout << "[C++] Python으로 보낼 데이터: " << sendData << endl;

    // 1. 길이 전송
    uint32_t len = htonl((uint32_t)sendData.size());
    if (!sendAll(python_fd_, (char*)&len, sizeof(len))) {
        cerr << "[C++] Python 길이 전송 실패" << endl;
        return R"({"protocol":"100_2","TEXT":"Send length failed"})"; // 실패
    }

    // 2. 본문 전송
    if (!sendAll(python_fd_, sendData.c_str(), (int)sendData.size())) {
        cerr << "[C++] Python 데이터 전송 실패" << endl;
        return R"({"protocol":"100_2","TEXT":"Send data failed"})"; // 실패
    }
    std::cout << "[C++] Python 데이터 전송 완료 (" << sendData.size() << " bytes)" << endl;

    // 3. 응답 길이 수신
    uint32_t respLenNet;
    if (!recvAll(python_fd_, (char*)&respLenNet, sizeof(respLenNet))) {
        cerr << "[C++] Python 응답 길이 수신 실패" << endl;
        return R"({"protocol":"100_2","TEXT":"Recv length failed"})"; // 실패
    }
    uint32_t respLen = ntohl(respLenNet);

    // 응답 길이 유효성 체크
    if (respLen == 0 || respLen > 1024 * 1024) {
        cerr << "[C++] Python 응답 길이 비정상: " << respLen << endl;
        return R"({"protocol":"100_2","TEXT":"Invalid response length"})"; // 실패
    }

    // 4. 응답 본문 수신메
    string buffer(respLen, '\0');
    if (!recvAll(python_fd_, &buffer[0], respLen)) {
        cerr << "[C++] Python 응답 데이터 수신 실패" << endl;
        return R"({"protocol":"100_2","TEXT":"Recv data failed"})"; // 실패
    }

    std::cout << "[C++] Python 응답 수신 완료 (" << respLen << " bytes)" << endl;
    std::cout << "[C++] 받은 응답: " << buffer << endl;

    return buffer;// Python JSON 그대로 반환
}

// ============================================================
// 서버 시작
// ============================================================
bool TcpServer::start() {
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    // 1. DB 연결
    if (!db.connectDB("127.0.0.1", "root", "1234", "PLANIT", 3306)) {
        return false;
    }

    if (!createSocket()) return false;
    if (!bindSocket()) return false;
    if (!listenSocket()) return false;

    running_ = true;
    thread(&TcpServer::acceptClients, this).detach();
    std::cout << "[C++] 서버 포트 " << port_ << "에서 대기중..." << endl;
    return true;
}

// ============================================================
// 서버 정지
// ============================================================
void TcpServer::stop() {
    running_ = false;
    if (server_fd_ != INVALID_SOCKET) closesocket(server_fd_);
    if (python_fd_ != INVALID_SOCKET) closesocket(python_fd_);
    if (pythonReceiverThread_.joinable()) pythonReceiverThread_.join();
    lock_guard<mutex> lock(threadMutex_);
    for (auto& t : clientThreads_) if (t.joinable()) t.join();
    clientThreads_.clear();
    WSACleanup();
}

// ============================================================
// 클라이언트 접속 수락
// (첫 번째 접속은 Python으로 간주)
// ============================================================
void TcpServer::acceptClients() {
    while (running_) {
        sockaddr_in clientAddr{};
        int addrlen = sizeof(clientAddr);
        SOCKET client_fd = accept(server_fd_, (struct sockaddr*)&clientAddr, &addrlen);
        if (client_fd == INVALID_SOCKET) {
            cerr << "accept failed: " << WSAGetLastError() << endl;
            continue;
        }

        char ip_str[INET_ADDRSTRLEN];
        inet_ntop(AF_INET, &(clientAddr.sin_addr), ip_str, INET_ADDRSTRLEN);
        std::cout << "[C++] 새 연결: " << ip_str << endl;

        // 첫 번째 연결은 Python으로 처리
        if (python_fd_ == INVALID_SOCKET) {
            python_fd_ = client_fd;
            std::cout << "[C++] Python 연결 완료!" << endl;
        }
        else {
            // 이후 연결은 WPF 클라이언트로 처리
            thread(&TcpServer::handleClient, this, client_fd).detach();
        }
    }
}
