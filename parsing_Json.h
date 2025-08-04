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

using json = nlohmann::json;
using namespace std;

class TcpServer;

class RequestHandler {
public:
    RequestHandler(TcpServer* server); // 생성자에서 TcpServer 포인터 받기
    void process(int client_fd, int python_fd, const std::string& jsonStr, std::string client_ip);

private:
    TcpServer* server_;
    using HandlerFunc = std::function<void(int, int, const nlohmann::json&)>; // 프로토콜 번호에 따라 실행 할 함수 저장하는곳
    std::unordered_map<int, HandlerFunc> handlers; //Key-Value 구조를 가진 해시맵

    void handleProtocol1(int client_fd, int python_fd, const nlohmann::json& j);
    void handleProtocol2(int client_fd, int python_fd, const nlohmann::json& j);
    void handleProtocol3(int client_fd, int python_fd, const nlohmann::json& j);
    void handleProtocol4(int client_fd, int python_fd, const nlohmann::json& j);
    void handleProtocol5(int client_fd, int python_fd, const nlohmann::json& j);
    void handleProtocol6(int client_fd, int python_fd, const nlohmann::json& j);
    void handleProtocol7(int client_fd, int python_fd, const nlohmann::json& j);
    void handleProtocol8(int client_fd, int python_fd, const nlohmann::json& j);
    void handleProtocol9(int client_fd, int python_fd, const nlohmann::json& j);
    //void send_json(int client_fd, const json& jsonstr, const string& jsonstr_2);

    std::string NAME;
    std::string ip;
};