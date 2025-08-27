//#include "server.h"
//#include "parsing_Json.h"
//#include <winsock2.h>
//#include <ws2tcpip.h>
//#include <nlohmann/json.hpp>
//#include <iostream>
//#include <thread>
//#include <mutex>
//#include <windows.h>
//
//#pragma comment(lib, "ws2_32.lib")

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

// ==추가
bool recvAll(int sock, char* buf, int len) {
    int totalRead = 0;
    while (totalRead < len) {
        int r = recv(sock, buf + totalRead, len - totalRead, 0);
        if (r <= 0) return false; // 오류 or 연결 종료
        totalRead += r;
    }
    return true;
}

// ===== WPF 요청 처리 =====
void TcpServer::handleClient(int client_fd) {
    //char buffer[4096];
    //int bytes;
    std::cout << "김대업 보내기전" << std::endl;
    while (true) {
        // 1️. 길이(4바이트) 수신
        uint32_t msgLenNet;
        //bytes = recv(client_fd, (char*)&msgLenNet, sizeof(msgLenNet), MSG_WAITALL);

        if (!recvAll(client_fd, (char*)&msgLenNet, sizeof(msgLenNet))) {
            std::cerr << "[C++] 길이 수신 실패" << std::endl;
            break;
        }
        uint32_t msgLen = ntohl(msgLenNet);

        //std::cout << "김대업 보낸후 1번" << std::endl;
        //std::cout << recv << endl;
        //std::cout << bytes << ": 첫번째 바이트" << endl;

        //if (bytes <= 0) {
        //    if (bytes == 0)
        //        std::cout << "[C++] WPF 클라이언트 연결 종료" << std::endl;
        //    else
        //        std::cerr << "[C++] WPF 길이 수신 실패: " << WSAGetLastError() << std::endl;
        //    break;
        //}

        
        
        // 2️. JSON 본문 수신
        std::string jsonStr(msgLen, '\0');
        cout << "sssssss" << endl;
        //bytes = recv(client_fd, &jsonStr[0], msgLen, MSG_WAITALL);
        if (!recvAll(client_fd, &jsonStr[0], msgLen)) {
            std::cerr << "[C++] 본문 수신 실패" << std::endl;
            break;
        }

        std::cout << "[C++] 받은 JSON: " << jsonStr << std::endl;

        //std::cout << "김대업 보낸후 2번" << std::endl; 
        //std::cout << jsonStr << endl;
        //std::cout << bytes << ": 두번째 바이트" << endl;

        //if (bytes <= 0) {
        //    std::cerr << "[C++] WPF 본문 수신 실패: " << WSAGetLastError() << std::endl;
        //    break;
        //}

        //std::cout << "[C++] WPF로부터 받은 JSON: " << jsonStr << std::endl;



        // 3️. Python에 전달 + 응답 수신
        std::string aiResponse = sendToPythonAndReceive(jsonStr);

        // 4️. Python 응답 길이(4바이트) + 본문 전송
        uint32_t respLenNet = htonl((uint32_t)aiResponse.size());
        bytes = send(client_fd, (char*)&respLenNet, sizeof(respLenNet), 0);
        if (bytes <= 0) {
            std::cerr << "[C++] WPF 응답 길이 전송 실패: " << WSAGetLastError() << std::endl;
            break;
        }
        bytes = send(client_fd, aiResponse.c_str(), (int)aiResponse.size(), 0);
        if (bytes <= 0) {
            std::cerr << "[C++] WPF 응답 데이터 전송 실패: " << WSAGetLastError() << std::endl;
            break;
        }

        std::cout << "[C++] WPF로 응답 전송 완료" << std::endl;
    }

    closesocket(client_fd);
    std::cout << "[C++] WPF 클라이언트 소켓 종료" << std::endl;
}



//~~~~테스트테스트테스트~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

// ===== 전체 데이터 전송 함수 =====
bool sendAll(SOCKET sock, const char* data, int length) {
    int totalSent = 0;
    while (totalSent < length) {
        int sent = send(sock, data + totalSent, length - totalSent, 0);
        if (sent <= 0) return false; // 오류 or 연결 종료
        totalSent += sent;
    }
    return true;
}

// ===== 정확한 길이만큼 수신 함수 =====
bool recvAll(SOCKET sock, char* buffer, int length) {
    int totalRecv = 0;
    while (totalRecv < length) {
        int received = recv(sock, buffer + totalRecv, length - totalRecv, 0);
        if (received <= 0) return false; // 오류 or 연결 종료
        totalRecv += received;
    }
    return true;
}

// ===== Python에 요청 보내고 응답 받기 =====
string TcpServer::sendToPythonAndReceive(const string& jsonStr) {

    // Python 소켓이 연결되어 있는지 확인
    if (python_fd_ == INVALID_SOCKET) {
        cerr << u8"[C++] Python 미연결 상태!" << endl;
        return R"({"PROTOCOL":999,"TEXT":"Python not connected"})";
    }

    // [테스트용] 만약 jsonStr이 비어있으면 기본 테스트 JSON 전송
    string sendData = jsonStr.empty()
        ? R"({"PROTOCOL":0,"TEXT":"PING TEST"})" // 기본 테스트 요청
        : jsonStr;

    cout << u8"[C++] Python으로 보낼 데이터: " << sendData << endl;

    // ===== 1. 길이(4바이트) 전송 =====
    uint32_t len = htonl((uint32_t)sendData.size());
    if (!sendAll(python_fd_, (char*)&len, sizeof(len))) {
        cerr << u8"[C++] Python 길이 전송 실패" << endl;
        return "";
    }
    // ===== 2. 본문 전송 =====
    if (!sendAll(python_fd_, sendData.c_str(), (int)sendData.size())) {
        cerr << u8"[C++] Python 데이터 전송 실패" << endl;
        return "";
    }
    cout << u8"[C++] Python 데이터 전송 완료 (" << sendData.size() << u8" bytes)" << endl;

    // ===== 3. 응답 길이 수신 =====
    uint32_t respLenNet;
    if (!recvAll(python_fd_, (char*)&respLenNet, sizeof(respLenNet))) {
        cerr << u8"[C++] Python 응답 길이 수신 실패" << endl;
        return "";
    }
    uint32_t respLen = ntohl(respLenNet);

    // ===== 길이 검증 =====
    if (respLen == 0 || respLen > 1024 * 1024) { // 10MB 이상이면 비정상
        cerr << u8"[C++] Python 응답 길이 비정상: " << respLen << endl;
        return "";
    }

    // ===== 4. 응답 본문 수신 =====
    string buffer(respLen, '\0');
    if (!recvAll(python_fd_, &buffer[0], respLen)) {
        cerr << u8"[C++] Python 응답 데이터 수신 실패" << endl;
        return "";
    }

    cout << u8"[C++] Python 응답 수신 완료 (" << respLen << u8" bytes)" << endl;
    cout << u8"[C++] 받은 응답: " << buffer << endl;
    return buffer;

}

// ===== 서버 시작 =====
bool TcpServer::start() {

    SetConsoleOutputCP(CP_UTF8);
    SetConsoleCP(CP_UTF8);

    if (!createSocket()) return false;
    if (!bindSocket()) return false;
    if (!listenSocket()) return false;

    running_ = true;
    thread(&TcpServer::acceptClients, this).detach();
    cout << u8"[C++] 서버 포트 " << port_ << u8"에서 대기중..." << endl;
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
        cout << u8"[C++] 새 연결: " << ip_str << endl;

        if (python_fd_ == INVALID_SOCKET) {
            python_fd_ = client_fd;
            cout << u8"[C++] Python 연결 완료!" << endl;
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
