//#include <winsock2.h>
//#include <ws2tcpip.h>
//#include <vector>
//#include <thread>
//#include <iostream>
//#include <nlohmann/json.hpp>
//#include <mysql.h>
//#include <locale.h>
//#include <cpprest/http_client.h> // GPT API 호출용
//#include <cpprest/filestream.h>
//
//#pragma comment(lib, "ws2_32.lib")
//
//using json = nlohmann::json;
//using namespace std;
//
//// ========================== OpenAI API 키 ==========================
//const std::string OPENAI_API_KEY = "여기에_네_API_KEY"; // <-- 여기에 본인 GPT API 키
//
//// ========================== DB 접속 정보 ==========================
//const char* HOST = "127.0.0.1";
//const char* USER = "guest";
//const char* PASS = "1234";
//const char* DB = "PLANIT";
//constexpr int PORT = 9000; // 서버 포트
//
//// ========================== DB 연결 함수 ==========================
//MYSQL* connect_db() {
//    MYSQL* conn = mysql_init(nullptr);
//    if (!mysql_real_connect(conn, HOST, USER, PASS, DB, 0, NULL, 0)) {
//        cerr << "DB 연결 실패: " << mysql_error(conn) << endl;
//        return nullptr;
//    }
//    mysql_set_character_set(conn, "utf8mb4");
//    cout << "DB 연결 성공" << endl;
//    return conn;
//}
//
//// ========================== 로그인 처리 함수 ==========================
//json handle_login(const json& data) {
//    json result;
//    MYSQL* conn = connect_db();
//    if (!conn) {
//        result["status"] = "fail";
//        result["message"] = "DB연결실패";
//        return result;
//    }
//
//    string U_ID = data.value("U_ID", "");
//
//    const char* sql =
//        "SELECT M_NAME, M_PRICE "
//        "FROM MENU_INFO "
//        "WHERE REPLACE(REPLACE(TRIM(M_NAME), ' ', ''), '\t', '') "
//        "  = REPLACE(REPLACE(TRIM(?), ' ', ''), '\t', '')";
//
//    MYSQL_STMT* stmt = mysql_stmt_init(conn);
//    if (!stmt || mysql_stmt_prepare(stmt, sql, strlen(sql)) != 0) {
//        result["status"] = "error";
//        result["message"] = stmt ? mysql_stmt_error(stmt) : mysql_error(conn);
//        if (stmt) mysql_stmt_close(stmt);
//        mysql_close(conn);
//        return result;
//    }
//
//    MYSQL_BIND bind[1] = {};
//    bind[0].buffer_type = MYSQL_TYPE_STRING;
//    bind[0].buffer = (char*)U_ID.c_str();
//    bind[0].buffer_length = U_ID.length();
//    mysql_stmt_bind_param(stmt, bind);
//
//    mysql_stmt_execute(stmt);
//
//    MYSQL_BIND resultBind[2] = {};
//    char nameBuf[128];
//    double priceBuf;
//    unsigned long nameLen;
//
//    resultBind[0].buffer_type = MYSQL_TYPE_STRING;
//    resultBind[0].buffer = nameBuf;
//    resultBind[0].buffer_length = sizeof(nameBuf);
//    resultBind[0].length = &nameLen;
//
//    resultBind[1].buffer_type = MYSQL_TYPE_DOUBLE;
//    resultBind[1].buffer = &priceBuf;
//
//    mysql_stmt_bind_result(stmt, resultBind);
//
//    if (mysql_stmt_fetch(stmt) == 0) {
//        nameBuf[nameLen] = '\0';
//        result["status"] = "success";
//        result["M_NAME"] = string(nameBuf);
//        result["M_PRICE"] = priceBuf;
//    }
//    else {
//        result["status"] = "fail";
//        result["message"] = "메뉴 없음";
//    }
//
//    mysql_stmt_close(stmt);
//    mysql_close(conn);
//    return result;
//}
//
//// ========================== GPT API 호출 함수 ==========================
//json call_gpt_api(const string& userMessage) {
//    try {
//        // 1. 요청 바디 생성 (nlohmann 사용)
//        json reqBody;
//        reqBody["model"] = "gpt-4o-mini";
//        reqBody["messages"] = json::array({
//            { {"role", "user"}, {"content", userMessage} }
//            });
//
//        // 2. JSON → string 변환
//        string bodyStr = reqBody.dump();
//
//        // 3. HTTP 요청 생성
//        web::http::client::http_client client(U("https://api.openai.com/v1/chat/completions"));
//        web::http::http_request req(web::http::methods::POST);
//        req.headers().add(U("Content-Type"), U("application/json"));
//        req.headers().add(U("Authorization"), U("Bearer " + OPENAI_API_KEY));
//        req.set_body(bodyStr, "application/json");
//
//        // 4. 요청 전송 및 응답 받기
//        auto res = client.request(req).get();
//        auto resStr = res.to_string();
//
//        // 5. 응답 JSON 파싱
//        json resJson = json::parse(resStr);
//        return resJson;
//    }
//    catch (const std::exception& e) {
//        json err;
//        err["status"] = "error";
//        err["message"] = e.what();
//        return err;
//    }
//}
//
//// ========================== 클라이언트 요청 처리 ==========================
//void handleClient(SOCKET clientSocket) {
//    char buffer[4096] = { 0 };
//    int recvLen = recv(clientSocket, buffer, sizeof(buffer) - 1, 0);
//    if (recvLen <= 0) {
//        closesocket(clientSocket);
//        return;
//    }
//    buffer[recvLen] = '\0';
//
//    try {
//        json request = json::parse(buffer);
//        json response;
//
//        if (request.contains("action")) {
//            string action = request["action"];
//            if (action == "login") {
//                response = handle_login(request["data"]);
//            }
//            else if (action == "gpt") {
//                string prompt = request["data"].value("prompt", "");
//                response = call_gpt_api(prompt);
//            }
//            else {
//                response["status"] = "fail";
//                response["message"] = "알 수 없는 요청";
//            }
//        }
//
//        string responseStr = response.dump();
//        send(clientSocket, responseStr.c_str(), static_cast<int>(responseStr.size()), 0);
//    }
//    catch (const exception& e) {
//        cerr << "처리 오류: " << e.what() << endl;
//    }
//
//    closesocket(clientSocket);
//}
//
//// ========================== 메인 함수 ==========================
//int main() {
//    setlocale(LC_ALL, "");
//
//    WSADATA wsa;
//    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) {
//        cerr << "WSAStartup 실패" << endl;
//        return 1;
//    }
//
//    SOCKET listenSocket = socket(AF_INET, SOCK_STREAM, 0);
//    if (listenSocket == INVALID_SOCKET) {
//        cerr << "소켓 생성 실패" << endl;
//        WSACleanup();
//        return 1;
//    }
//
//    sockaddr_in serverAddr{};
//    serverAddr.sin_family = AF_INET;
//    serverAddr.sin_port = htons(PORT);
//    serverAddr.sin_addr.s_addr = INADDR_ANY;
//
//    if (::bind(listenSocket, (sockaddr*)&serverAddr, sizeof(serverAddr)) == SOCKET_ERROR) {
//        cerr << "bind 실패" << endl;
//        closesocket(listenSocket);
//        WSACleanup();
//        return 1;
//    }
//
//    if (listen(listenSocket, SOMAXCONN) == SOCKET_ERROR) {
//        cerr << "listen 실패" << endl;
//        closesocket(listenSocket);
//        WSACleanup();
//        return 1;
//    }
//
//    cout << "서버 시작: 포트 " << PORT << " 대기 중..." << endl;
//
//    while (true) {
//        SOCKET client = accept(listenSocket, NULL, NULL);
//        if (client == INVALID_SOCKET) continue;
//        thread th(handleClient, client);
//        th.detach();
//    }
//
//    closesocket(listenSocket);
//    WSACleanup();
//    return 0;
//}
