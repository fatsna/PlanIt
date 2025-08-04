#include "parsing_Json.h"
#include "server.h"
#include <winsock2.h>
#include <nlohmann/json.hpp>
#include <iostream>

using json = nlohmann::json;

//생성자 구현
RequestHandler::RequestHandler(TcpServer* server)
    : server_(server) {
    // 필요하다면 프로토콜 핸들러 초기화
    // 예: handlers[2] = [this](int fd, int py_fd, const json& j) { handleProtocol2(fd, py_fd, j); };
}

void RequestHandler::process(int client_fd, int python_fd, const std::string& jsonStr, std::string client_ip) {
    std::cout << "[RequestHandler] process 호출됨" << std::endl;
    std::cout << "Client FD: " << client_fd << ", Python FD: " << python_fd << std::endl;
    std::cout << "Client IP: " << client_ip << std::endl;
    std::cout << "받은 JSON: " << jsonStr << std::endl;

    // TcpServer 멤버 함수 호출
    std::string aiResponse = server_->sendToPythonAndReceive(jsonStr);

    int sendResult = send(client_fd, aiResponse.c_str(), (int)aiResponse.size(), 0);
    if (sendResult == SOCKET_ERROR) {
        std::cerr << "[오류] 클라이언트 응답 전송 실패: " << WSAGetLastError() << std::endl;
    }
    else {
        std::cout << "[성공] 클라이언트로 응답 전송 완료" << std::endl;
    }
}




//#include "parsing_Json.h"
//#include "server.h"
//#include <winsock2.h>
//#include <iostream>
////#include "db_class.h"
//
//using json = nlohmann::json;
//
////등록을한다. 함수를 목록화
//RequestHandler::RequestHandler(TcpServer* server)
//    : server_(server)
//{ 
//    //handlers[1] = [this](int fd, int py_fd, const json& j) { handleProtocol1(fd, py_fd, j); };
//    handlers[2] = [this](int fd, int py_fd, const json& j) { handleProtocol2(fd, py_fd, j); };
//    //handlers[3] = [this](int fd, int py_fd, const json& j) { handleProtocol3(fd, py_fd, j); };
//    //handlers[4] = [this](int fd, int py_fd, const json& j) { handleProtocol4(fd, py_fd, j); };
//    //handlers[5] = [this](int fd, int py_fd, const json& j) { handleProtocol5(fd, py_fd, j); };
//    //handlers[6] = [this](int fd, int py_fd, const json& j) { handleProtocol6(fd, py_fd, j); };
//    //handlers[7] = [this](int fd, int py_fd, const json& j) { handleProtocol7(fd, py_fd, j); };
//    //handlers[8] = [this](int fd, int py_fd, const json& j) { handleProtocol8(fd, py_fd, j); };
//    //handlers[10] = [this](int fd, int py_fd, const json& j) { handleProtocol9(fd, py_fd, j); };
//}
//
//void RequestHandler::handleProtocol2(int fd, int py_fd, const json& j) {
//
//    std::string text = j["TEXT"].get<std::string>(); // 키값(TEXT)를 꺼내서 문자열 타입으로 반환
//    
//    // Python 서버로 보낼 JSON구성
//    json msg_j = {                         
//        {"PROTOCOL", "2"},
//        {"CLIENT_FD", to_string(fd)},
//        //{"CLIENT_IP", NAME},               // 클라이언트 이름 또는 IP 저장 변수
//        //{"CLIENT_NAME", NAME},
//        {"TEXT", text}                     // 클라이언트가 보낸 원문 텍스트
//    };
//
//    std::string msg = msg_j.dump();             // JSON 객체 → 문자열변환
//
//    int sent = send((SOCKET)py_fd, msg.c_str(), static_cast<int>(msg.size()), 0);
//
//    if (sent == SOCKET_ERROR || sent != static_cast<int>(msg.size())){
//        std::cerr << "[Python] 텍스트 전송 실패 (send 오류)" << WSAGetLastError() << std::endl;
//    }
//    else {
//        cout << "[Python] 텍스트 전송 완료: " << msg << endl;
//    }
//}
//void RequestHandler::process(int client_fd, int python_fd, const std::string& jsonStr, std::string client_ip) {
//    std::cout << "[RequestHandler] process 호출됨" << std::endl;
//    std::cout << "Client FD: " << client_fd << ", Python FD: " << python_fd << std::endl;
//    std::cout << "Client IP: " << client_ip << std::endl;
//    std::cout << "받은 JSON: " << jsonStr << std::endl;
//
//    std::string aiResponse = server_->sendToPythonAndReceive(jsonStr); // 여기서 호출
//    int sendResult = send(client_fd, aiResponse.c_str(), (int)aiResponse.size(), 0);
//    if (sendResult == SOCKET_ERROR) {
//        std::cerr << "[오류] 클라이언트 응답 전송 실패: " << WSAGetLastError() << std::endl;
//    }
//    else {
//        std::cout << "[성공] 클라이언트로 응답 전송 완료" << std::endl;
//    }
//}
