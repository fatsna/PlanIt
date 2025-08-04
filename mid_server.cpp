#include "server.h"
#include "parsing_Json.h"
#include <winsock2.h>
#include <ws2tcpip.h>
#include <nlohmann/json.hpp>
#include <iostream>
#include <thread>
#include <mutex>

#pragma comment(lib, "ws2_32.lib")

using namespace std;

TcpServer::TcpServer(int port)
    : port_(port), server_fd_(INVALID_SOCKET), python_fd_(INVALID_SOCKET), running_(false) {
}

TcpServer::~TcpServer() {
    stop();
}

// ===== 소켓 생성 =====
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

// ===== 소켓 바인딩 =====
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

// ===== 클라이언트 연결 대기 시작 =====
bool TcpServer::listenSocket() {
    return listen(server_fd_, 5) != SOCKET_ERROR;
}

// ===== WPF 요청 처리 =====
void TcpServer::handleClient(int client_fd) {
    char buffer[4096];
    int bytes;

    // TcpServer 포인터 넘겨서 RequestHandler 생성
    RequestHandler handler(this);

    while ((bytes = recv(client_fd, buffer, sizeof(buffer), 0)) > 0) {
        string jsonStr(buffer, bytes);
        cout << "[C++] WPF로부터 받은 JSON: " << jsonStr << endl;
        handler.process(client_fd, python_fd_, jsonStr, client_ip_);
    }

    if (bytes == SOCKET_ERROR) {
        cerr << "recv failed: " << WSAGetLastError() << endl;
    }
    else if (bytes == 0) {
        cout << "Client disconnected gracefully." << endl;
    }
    closesocket(client_fd);
    cout << "Client disconnected." << endl;
}

// ===== Python에 요청 보내고 응답 받기 =====
//string TcpServer::sendToPythonAndReceive(const string& jsonStr) {
//    if (python_fd_ == INVALID_SOCKET) {
//        cerr << "[C++] Python 미연결 상태!" << endl;
//        return R"({"PROTOCOL":999,"TEXT":"Python not connected"})";
//    }
//
//    uint32_t len = htonl((uint32_t)jsonStr.size());
//    send(python_fd_, (char*)&len, sizeof(len), 0);
//    send(python_fd_, jsonStr.c_str(), (int)jsonStr.size(), 0);
//
//    uint32_t respLenNet;
//    if (recv(python_fd_, (char*)&respLenNet, sizeof(respLenNet), MSG_WAITALL) <= 0) {
//        cerr << "[C++] Python 응답 길이 수신 실패" << endl;
//        return "";
//    }
//    uint32_t respLen = ntohl(respLenNet);
//
//    string buffer(respLen, '\0');
//    recv(python_fd_, &buffer[0], respLen, MSG_WAITALL);
//    return buffer;
//}


//테스트테스트테스트
// ===== Python에 요청 보내고 응답 받기 =====
string TcpServer::sendToPythonAndReceive(const string& jsonStr) {
    // Python 소켓이 연결되어 있는지 확인
    if (python_fd_ == INVALID_SOCKET) {
        cerr << "[C++] Python 미연결 상태!" << endl;
        return R"({"PROTOCOL":999,"TEXT":"Python not connected"})";
    }

    // [테스트용] 만약 jsonStr이 비어있으면 기본 테스트 JSON 전송
    string sendData = jsonStr.empty()
        ? R"({"PROTOCOL":0,"TEXT":"PING TEST"})" // 기본 테스트 요청
        : jsonStr;

    cout << "[C++] Python으로 보낼 데이터: " << sendData << endl;

    // ===== 1. 데이터 길이(4바이트) 전송 =====
    uint32_t len = htonl((uint32_t)sendData.size());
    int sentLen = send(python_fd_, (char*)&len, sizeof(len), 0);
    if (sentLen <= 0) {
        cerr << "[C++] Python 길이 전송 실패: " << WSAGetLastError() << endl;
        return "";
    }

    // ===== 2. 실제 데이터 전송 =====
    int sentData = send(python_fd_, sendData.c_str(), (int)sendData.size(), 0);
    if (sentData <= 0) {
        cerr << "[C++] Python 데이터 전송 실패: " << WSAGetLastError() << endl;
        return "";
    }
    cout << "[C++] Python으로 데이터 전송 완료 (" << sentData << " bytes)" << endl;

    // ===== 3. Python 응답 길이(4바이트) 수신 =====
    uint32_t respLenNet;
    int recvLen = recv(python_fd_, (char*)&respLenNet, sizeof(respLenNet), MSG_WAITALL);
    if (recvLen <= 0) {
        cerr << "[C++] Python 응답 길이 수신 실패" << endl;
        return "";
    }
    uint32_t respLen = ntohl(respLenNet);

    // ===== 4. Python 응답 본문 수신 =====
    string buffer(respLen, '\0');
    int recvData = recv(python_fd_, &buffer[0], respLen, MSG_WAITALL);
    if (recvData <= 0) {
        cerr << "[C++] Python 응답 데이터 수신 실패" << endl;
        return "";
    }

    cout << "[C++] Python 응답 수신 완료 (" << recvData << " bytes)" << endl;
    cout << "[C++] 받은 응답: " << buffer << endl;

    return buffer;
}

// ===== 서버 시작 =====
bool TcpServer::start() {
    if (!createSocket()) return false;
    if (!bindSocket()) return false;
    if (!listenSocket()) return false;

    running_ = true;
    thread(&TcpServer::acceptClients, this).detach();
    cout << "[C++] 서버 포트 " << port_ << "에서 대기중..." << endl;
    return true;
}

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

// ===== 클라이언트 접속 수락 =====
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
        cout << "[C++] 새 연결: " << ip_str << endl;

        if (python_fd_ == INVALID_SOCKET) {
            python_fd_ = client_fd;
            cout << "[C++] Python 연결 완료!" << endl;
            string jsonStr;
            string sendData = jsonStr.empty()
                ? R"({"PROTOCOL":0,"TEXT":"3+3=?"})" // 기본 테스트 요청
                : jsonStr;

            sendToPythonAndReceive(sendData);
        }
        else {
            thread(&TcpServer::handleClient, this, client_fd).detach();
        }
    }
}



//#include "server.h"
//#include "parsing_Json.h"
//// Windows 전용
//#include <winsock2.h>
//#include <ws2tcpip.h>
//#pragma comment(lib, "ws2_32.lib")
//#include <cstring>
//#include <fstream>
//#include <nlohmann/json.hpp>
//using namespace std;
//
//TcpServer::TcpServer(int port)
//    : port_(port), server_fd_(INVALID_SOCKET), python_fd_(INVALID_SOCKET), running_(false) {
//}
//
//TcpServer::~TcpServer() {
//    stop();
//}
//
//// ===== 소켓 생성 =====
//bool TcpServer::createSocket() {
//    WSADATA wsaData;
//    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
//        cerr << "WSAStartup failed: " << WSAGetLastError() << endl;
//        return false;
//    }
//    server_fd_ = socket(AF_INET, SOCK_STREAM, 0);
//    if (server_fd_ == INVALID_SOCKET) {
//        cerr << "socket 실패: " << WSAGetLastError() << endl;
//        WSACleanup();
//        return false;
//    }
//    return true;
//}
//
//// ===== 소켓 바인딩 =====
//bool TcpServer::bindSocket() {
//    sockaddr_in address{};
//    address.sin_family = AF_INET;
//    address.sin_addr.s_addr = INADDR_ANY;
//    address.sin_port = htons(port_);
//
//    int opt = 1;
//    setsockopt(server_fd_, SOL_SOCKET, SO_REUSEADDR, (const char*)&opt, sizeof(opt));
//
//    if (::bind(server_fd_, reinterpret_cast<struct sockaddr*>(&address), sizeof(address)) == SOCKET_ERROR) {
//        cerr << "bind failed: " << WSAGetLastError() << endl;
//        return false;
//    }
//    return true;
//}
//
//// ===== 클라이언트 연결 대기 시작 =====
//bool TcpServer::listenSocket() {
//    return listen(server_fd_, 5) != SOCKET_ERROR;
//}
//
//// [삭제] Python AI 서버에 직접 연결하는 코드 (connectToPythonServer) 완전히 제거
//
//// ===== WPF 요청 처리 =====
//void TcpServer::handleClient(int client_fd) { // [수정] 기존 int → SOCKET로 통일
//    char buffer[4096];
//    int bytes;
//
//    // 서버 포인터 넘겨서 RequestHandler 생성
//    RequestHandler handler(this);
//
//    while ((bytes = recv(client_fd, buffer, sizeof(buffer), 0)) > 0) {
//        std::string jsonStr(buffer, bytes);
//        cout << "[C++] WPF로부터 받은 JSON: " << jsonStr << endl;
//        std::cout << "client_fd: " << client_fd << std::endl;
//
//        //// [수정] 기존 connectToPythonServer() 대신 이미 연결된 python_fd_ 사용
//        //string aiResponse = sendToPythonAndReceive(jsonStr);
//        //send(client_fd, aiResponse.c_str(), (int)aiResponse.size(), 0);
//        // 
//        // JSON 처리
//        handler.process(client_fd, python_fd_, jsonStr, client_ip_);
//    }
//    if (bytes == SOCKET_ERROR) {
//        std::cerr << "recv failed: " << WSAGetLastError() << std::endl;
//    }
//    else if (bytes == 0) {
//        std::cout << "Client disconnected gracefully." << std::endl;
//    }
//    closesocket(client_fd);
//    std::cout << "Client disconnected." << std::endl;
//}
//
//// ===== Python에 요청 보내고 응답 받기 =====
//string TcpServer::sendToPythonAndReceive(const string& jsonStr) { // [수정] python_fd_ 재사용
//    if (python_fd_ == INVALID_SOCKET) {
//        cerr << "[C++] Python 미연결 상태!" << endl;
//        return R"({"PROTOCOL":999,"TEXT":"Python not connected"})";
//    }
//
//    uint32_t len = htonl((uint32_t)jsonStr.size());
//    send(python_fd_, (char*)&len, sizeof(len), 0);
//    send(python_fd_, jsonStr.c_str(), (int)jsonStr.size(), 0);
//
//    uint32_t respLenNet;
//    if (recv(python_fd_, (char*)&respLenNet, sizeof(respLenNet), MSG_WAITALL) <= 0) {
//        cerr << "[C++] Python 응답 길이 수신 실패" << endl;
//        return "";
//    }
//    uint32_t respLen = ntohl(respLenNet);
//
//    string buffer(respLen, '\0');
//    recv(python_fd_, &buffer[0], respLen, MSG_WAITALL);
//    return buffer;
//}
//
//// ===== RequestHandler =====
//void RequestHandler::process(int client_fd, int python_fd, const std::string& jsonStr, std::string client_ip) {
//    std::cout << "[RequestHandler] process 호출됨" << std::endl;
//    std::cout << "Client FD: " << client_fd << ", Python FD: " << python_fd << std::endl;
//    std::cout << "Client IP: " << client_ip << std::endl;
//    std::cout << "받은 JSON: " << jsonStr << std::endl;
//
//    std::string aiResponse = serverInstance.sendToPythonAndReceive(jsonStr);
//
//    int sendResult = send(client_fd, aiResponse.c_str(), (int)aiResponse.size(), 0);
//    if (sendResult == SOCKET_ERROR) {
//        std::cerr << "[오류] 클라이언트 응답 전송 실패: " << WSAGetLastError() << std::endl;
//    }
//    else {
//        std::cout << "[성공] 클라이언트로 응답 전송 완료" << std::endl;
//    }
//}
//
//// ===== 서버 시작 =====
//bool TcpServer::start() {
//    if (!createSocket()) return false;
//    if (!bindSocket()) return false;
//    if (!listenSocket()) return false;
//
//    running_ = true;
//    thread(&TcpServer::acceptClients, this).detach(); // [수정] Python + WPF 모두 수락
//    cout << "[C++] 서버 포트 " << port_ << "에서 대기중..." << endl;
//    return true;
//}
//
//void TcpServer::stop() {
//    running_ = false;
//    if (server_fd_ != INVALID_SOCKET) closesocket(server_fd_);
//    if (python_fd_ != INVALID_SOCKET) closesocket(python_fd_);
//    if (pythonReceiverThread_.joinable()) pythonReceiverThread_.join();
//    lock_guard<mutex> lock(threadMutex_);
//    for (auto& t : clientThreads_) if (t.joinable()) t.join();
//    clientThreads_.clear();
//    WSACleanup();
//}
//
//// ===== 클라이언트 접속 수락 =====
//void TcpServer::acceptClients() {
//    while (running_) {
//        sockaddr_in clientAddr{};
//        int addrlen = sizeof(clientAddr);
//        SOCKET client_fd = accept(server_fd_, (struct sockaddr*)&clientAddr, &addrlen);
//        if (client_fd == INVALID_SOCKET) {
//            cerr << "accept failed: " << WSAGetLastError() << endl;
//            continue;
//        }
//
//        char ip_str[INET_ADDRSTRLEN];
//        inet_ntop(AF_INET, &(clientAddr.sin_addr), ip_str, INET_ADDRSTRLEN);
//        cout << "[C++] 새 연결: " << ip_str << endl;
//
//        // [수정] 첫 번째 연결을 Python 연결로 간주
//        if (python_fd_ == INVALID_SOCKET) {
//            python_fd_ = client_fd;
//            cout << "[C++] Python 연결 완료!" << endl;
//        }
//        else {
//            // [수정] 그 외 연결은 WPF 클라이언트 처리
//            thread(&TcpServer::handleClient, this, client_fd).detach();
//        }
//    }
//}
//



//
//
//// ===== 생성자 =====
//// 포트 번호를 받아서 초기화, 서버 소켓은 아직 생성 전(-1)
//TcpServer::TcpServer(int port)
//    : port_(port), server_fd_(INVALID_SOCKET), python_fd_(INVALID_SOCKET), running_(false) {
//}
//
//TcpServer::~TcpServer() {
//    stop();
//}
//
//// ===== 소켓 생성 =====
//bool TcpServer::createSocket() {
//
//    // Winsock 초기화
//    WSADATA wsaData;
//    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
//        cerr << "WSAStartup failed: " << WSAGetLastError() << endl;
//        return false;
//    }
//
//    server_fd_ = socket(AF_INET, SOCK_STREAM, 0);
//    if (server_fd_ == INVALID_SOCKET) {
//        cerr << "socket 실패: " << WSAGetLastError() << endl;
//        WSACleanup();
//        return false;
//    }
//    return true;
//}
//
//// ===== 소켓 바인딩 =====
//bool TcpServer::bindSocket() {
//    sockaddr_in address{};
//    address.sin_family = AF_INET;
//    address.sin_addr.s_addr = INADDR_ANY;
//    address.sin_port = htons(port_);
//
//    int opt = 1;
//    // 포트 재사용 옵션 설정 (서버 재시작 시 TIME_WAIT 방지)
//    setsockopt(server_fd_, SOL_SOCKET, SO_REUSEADDR, (const char*)&opt, sizeof(opt));
//
//    if (::bind(server_fd_, reinterpret_cast<struct sockaddr*>(&address), sizeof(address)) == SOCKET_ERROR) {
//        std::cerr << "bind failed: " << WSAGetLastError() << std::endl;
//        return false;
//    }
//    return true;
//}
//
//// ===== 클라이언트 연결 대기 시작 =====
//bool TcpServer::listenSocket() {
//    return listen(server_fd_, 5) != SOCKET_ERROR;
//}
//
//// ===== Python AI 서버 연결 =====
////bool TcpServer::connectToPythonServer(const string& host, int port) {
////    python_fd_ = socket(AF_INET, SOCK_STREAM, 0);
////    if (python_fd_ == INVALID_SOCKET) {
////        cerr << "[Python 연결] 소켓 생성 실패: " << WSAGetLastError() << endl;
////        return false;
////    }
////
////    sockaddr_in serv_addr{};
////    serv_addr.sin_family = AF_INET;
////    serv_addr.sin_port = htons(port);
////
////    if (inet_pton(AF_INET, host.c_str(), &serv_addr.sin_addr) <= 0) {
////        cerr << "[Python 연결] IP 변환 실패" << endl;
////        return false;
////    }
////
////    if (connect(python_fd_, (struct sockaddr*)&serv_addr, sizeof(serv_addr)) == SOCKET_ERROR) {
////        cerr << "[Python 연결] 실패: " << WSAGetLastError() << endl;
////        closesocket(python_fd_);
////        python_fd_ = INVALID_SOCKET;
////        return false;
////    }
////
////    cout << "[Python 연결] 성공: " << host << ":" << port << endl;
////    return true;
////}
//
//
//static const char* PYTHON_SERVER_IP = "127.0.0.1";
//static const int   PYTHON_SERVER_PORT = 5000;
//// WPF 요청 처리
//void TcpServer::handleClient(SOCKET client_fd) {
//    char buffer[4096];
//    int bytes;
//    while ((bytes = recv(client_fd, buffer, sizeof(buffer), 0)) > 0) {
//        string jsonStr(buffer, bytes);
//        cout << "[C++] WPF로부터 받은 JSON: " << jsonStr << endl;
//
//        // 이미 연결된 python_fd_ 사용
//        string aiResponse = sendToPythonAndReceive(jsonStr);
//
//        send(client_fd, aiResponse.c_str(), (int)aiResponse.size(), 0);
//    }
//    closesocket(client_fd);
//}
//
//// Python에 요청 보내고 응답 받기
//string TcpServer::sendToPythonAndReceive(const string& jsonStr) {
//    if (python_fd_ == INVALID_SOCKET) {
//        cerr << "[C++] Python 미연결 상태!" << endl;
//        return R"({"PROTOCOL":999,"TEXT":"Python not connected"})";
//    }
//
//    // 매번 새 소켓 생성하지 않고 python_fd_ 재사용
//    uint32_t len = htonl((uint32_t)jsonStr.size());
//    send(python_fd_, (char*)&len, sizeof(len), 0);
//    send(python_fd_, jsonStr.c_str(), (int)jsonStr.size(), 0);
//
//    uint32_t respLenNet;
//    if (recv(python_fd_, (char*)&respLenNet, sizeof(respLenNet), MSG_WAITALL) <= 0) {
//        cerr << "[C++] Python 응답 길이 수신 실패" << endl;
//        return "";
//    }
//    uint32_t respLen = ntohl(respLenNet);
//
//    string buffer(respLen, '\0');
//    recv(python_fd_, &buffer[0], respLen, MSG_WAITALL);
//    return buffer;
//}
////std::string sendToPythonAndReceive(const std::string& userMessage)
////{
////    SOCKET sock = socket(AF_INET, SOCK_STREAM, 0);
////    if (sock == INVALID_SOCKET) {
////        cerr << "[Python 연결] 소켓 생성 실패: " << WSAGetLastError() << endl;
////        return "";
////    }
////
////    sockaddr_in serv_addr{};
////    serv_addr.sin_family = AF_INET;
////    serv_addr.sin_port = htons(PYTHON_SERVER_PORT);
////    inet_pton(AF_INET, PYTHON_SERVER_IP, &serv_addr.sin_addr);
////
////    if (connect(sock, (sockaddr*)&serv_addr, sizeof(serv_addr)) == SOCKET_ERROR) {
////        cerr << "[Python 연결] 실패: " << WSAGetLastError() << endl;
////        closesocket(sock);
////        return "";
////    }
////
////    // 길이(4바이트) + 데이터 전송
////    uint32_t dataLen = htonl((uint32_t)userMessage.size());
////    send(sock, (char*)&dataLen, sizeof(dataLen), 0);
////    send(sock, userMessage.c_str(), (int)userMessage.size(), 0);
////
////    // 응답 길이 수신
////    uint32_t respLenNet;
////    if (recv(sock, (char*)&respLenNet, sizeof(respLenNet), MSG_WAITALL) <= 0) {
////        cerr << "[Python 연결] 응답 길이 수신 실패" << endl;
////        closesocket(sock);
////        return "";
////    }
////    uint32_t respLen = ntohl(respLenNet);
////
////    // 응답 본문 수신
////    std::string buffer(respLen, '\0');
////    int totalRead = 0;
////    while (totalRead < (int)respLen) {
////        int bytes = recv(sock, &buffer[totalRead], respLen - totalRead, 0);
////        if (bytes <= 0) break;
////        totalRead += bytes;
////    }
////
////    closesocket(sock);
////    return buffer;
////}
//
//void RequestHandler::process(int client_fd, int python_fd, const std::string& jsonStr, std::string client_ip)
//{
//    std::cout << "[RequestHandler] process 호출됨" << std::endl;
//    std::cout << "Client FD: " << client_fd << ", Python FD: " << python_fd << std::endl;
//    std::cout << "Client IP: " << client_ip << std::endl;
//    std::cout << "받은 JSON: " << jsonStr << std::endl;
//
//    // Python AI 서버로 요청 전송 및 응답 수신
//    std::string aiResponse = sendToPythonAndReceive(jsonStr);
//
//    // AI 서버 응답을 클라이언트로 전송
//    int sendResult = send(client_fd, aiResponse.c_str(), (int)aiResponse.size(), 0);
//    if (sendResult == SOCKET_ERROR) {
//        std::cerr << "[오류] 클라이언트 응답 전송 실패: " << WSAGetLastError() << std::endl;
//    }
//    else {
//        std::cout << "[성공] 클라이언트로 응답 전송 완료" << std::endl;
//    }
//}
//
//// ===== 서버 시작 =====
//bool TcpServer::start() {
//    if (!createSocket()) {
//        cerr << "createSocket 실패" << endl;
//        return false;
//    }
//    if (!bindSocket()) {
//        cerr << "bindSocket 실패" << endl;
//        return false;
//    }
//    if (!listenSocket()) {
//        cerr << "listenSocket 실패" << endl;
//        return false;
//    }
//
//    running_ = true;
//
//    // 클라이언트 접속 수락 스레드 실행 (백그라운드)
//    thread(&TcpServer::acceptClients, this).detach();
//
//    cout << "[C++] 서버 포트 " << port_ << "에서 대기중..." << endl;
//    return true;
//}
//
//void TcpServer::stop() {
//    running_ = false;
//    cout << "소켓닫고 스레드 종료대기";
//
//    // 서버 소켓 닫기
//    if (server_fd_ != INVALID_SOCKET) {
//        closesocket(server_fd_);
//        server_fd_ = INVALID_SOCKET;
//    }
//    // 파이썬 서버와 연결된 소켓 닫기
//    if (python_fd_ != INVALID_SOCKET) {
//        closesocket(python_fd_);
//        python_fd_ = INVALID_SOCKET;
//    }
//    // 파이썬 수신 스레드 종료 대기
//    if (pythonReceiverThread_.joinable()) {
//        pythonReceiverThread_.join();
//    }
//    // 모든 클라이언트 스레드 종료 대기
//    lock_guard<mutex> lock(threadMutex_);
//    for (auto& t : clientThreads_) {
//        if (t.joinable())
//            t.join();
//    }
//    clientThreads_.clear();
//
//    // Winsock 종료
//    WSACleanup();
//}
//
//// ===== 클라이언트 접속 수락 =====
//void TcpServer::acceptClients() {
//    while (running_) {
//        sockaddr_in clientAddr{};
//        //socklen_t addrlen = sizeof(clientAddr);
//        int addrlen = sizeof(clientAddr);
//
//        // 클라이언트 연결 대기
//        SOCKET client_fd = accept(server_fd_, (struct sockaddr*)&clientAddr, &addrlen);
//        if (client_fd == INVALID_SOCKET) {
//            cerr << "accept failed: " << WSAGetLastError() << endl;
//            continue;
//        }
//        // 클라이언트 IP 문자열로 변환
//        char ip_str[INET_ADDRSTRLEN];
//        inet_ntop(AF_INET, &(clientAddr.sin_addr), ip_str, INET_ADDRSTRLEN);
//        cout << "[C++] 새 연결: " << ip_str << endl;
//
//        // [수정] 첫 번째 연결을 Python 연결로 간주
//        if (python_fd_ == INVALID_SOCKET) {
//            python_fd_ = client_fd;
//            cout << "[C++] Python 연결 완료!" << endl;
//        }
//        else {
//            // [수정] 그 외 연결은 WPF 클라이언트 처리
//            thread(&TcpServer::handleClient, this, client_fd).detach();
//        }
//        
//        //client_ip_ = ip_str; // 멤버 변수에 저장
//        //// 클라이언트 전담 스레드 생성
//        //lock_guard<mutex> lock(threadMutex_);
//        //clientThreads_.emplace_back(&TcpServer::handleClient, this, client_fd);
//    }
//}
//
//// ===== 클라이언트 요청 처리 =====
//void TcpServer::handleClient(int client_fd) {
//    char buffer[1024];
//    int bytesRead;
//    // JSON 요청 처리 클래스
//    RequestHandler handler;
//
//    // 데이터 수신 루프
//    while ((bytesRead = recv(client_fd, buffer, sizeof(buffer), 0)) > 0) {
//        string message(buffer, bytesRead);
//        cout << "Received: " << message << endl;
//        cout << "client_fd: " << client_fd << endl;
//
//        // JSON 파싱 및 처리
//        handler.process(client_fd, python_fd_, message, client_ip_);
//    }
//
//
//
//    if (bytesRead == SOCKET_ERROR) {
//        cerr << "recv failed: " << WSAGetLastError() << endl;
//    }
//    else if (bytesRead == 0) {
//        cout << "Client disconnected gracefully." << endl;
//    }
//
//    closesocket(client_fd);
//    cout << "Client disconnected." << endl;
//}
