//#include <winsock2.h>
//#include <ws2tcpip.h>
//#include <vector>
//#include <thread>
//#include <iostream>
//#include <nlohmann/json.hpp>
//#include <mysql.h>
//#include <locale.h>
//#include <string>
//
//#pragma comment(lib, "ws2_32.lib")
//#pragma comment(lib, "libmysql.lib") // MySQL 라이브러리 링크 필요
//
//using json = nlohmann::json;
//using namespace std;
//
//// ========================== DB 접속 정보 ==========================
//const char* HOST = "127.0.0.1";
//const char* USER = "guest";
//const char* PASS = "1234";
//const char* DB = "PLANIT";
//constexpr int PORT = 9000;     // C++ 서버 포트
//constexpr int AI_PORT = 5000;  // Python AI 서버 포트
//
//// ========================== DB 연결 함수 ==========================
//MYSQL* connect_db() {
//    MYSQL* conn = mysql_init(nullptr);
//    if (!mysql_real_connect(conn, HOST, USER, PASS, DB, 0, NULL, 0)) {
//        cerr << "DB 연결 실패: " << mysql_error(conn) << endl;
//        return nullptr;
//    }
//    mysql_set_character_set(conn, "utf8mb4");
//    return conn;
//}
//
//// =============== 로그인 처리 (Prepared Statement) ============
//json handle_login(const json& data) {
//    json result;
//    MYSQL* conn = connect_db();
//    if (!conn) {
//        result["status"] = "fail";
//        result["message"] = "DB 연결 실패";
//        return result;
//    }
//
//    string userId = data.value("U_ID", "");
//    string password = data.value("U_PW", "");
//
//    // Prepared Statement 사용 → SQL 인젝션 방지
//    const char* query = "SELECT U_ID FROM USER_INFO WHERE U_ID=? AND U_PW=?";
//
//    MYSQL_STMT* stmt = mysql_stmt_init(conn);
//    mysql_stmt_prepare(stmt, query, strlen(query));
//
//    MYSQL_BIND bind[2] = {};
//    bind[0].buffer_type = MYSQL_TYPE_STRING;
//    bind[0].buffer = (char*)userId.c_str();
//    bind[0].buffer_length = userId.length();
//
//    bind[1].buffer_type = MYSQL_TYPE_STRING;
//    bind[1].buffer = (char*)password.c_str();
//    bind[1].buffer_length = password.length();
//
//    mysql_stmt_bind_param(stmt, bind);
//    mysql_stmt_execute(stmt);
//
//    MYSQL_RES* meta = mysql_stmt_result_metadata(stmt);
//    MYSQL_BIND resultBind[1] = {};
//
//    char idBuffer[50];
//    unsigned long idLen;
//    resultBind[0].buffer_type = MYSQL_TYPE_STRING;
//    resultBind[0].buffer = idBuffer;
//    resultBind[0].buffer_length = sizeof(idBuffer);
//    resultBind[0].length = &idLen;
//
//    mysql_stmt_bind_result(stmt, resultBind);
//
//    if (mysql_stmt_fetch(stmt) == 0) {
//        result["status"] = "success";
//        result["U_ID"] = string(idBuffer, idLen);
//    }
//    else {
//        result["status"] = "fail";
//        result["message"] = "아이디 또는 비밀번호 불일치";
//    }
//
//    mysql_free_result(meta);
//    mysql_stmt_close(stmt);
//    mysql_close(conn);
//    return result;
//}
//
//// ========================== AI 서버 호출 함수 (TCP 안정성 강화) ==========================
//string ask_ai_server(const string& userMessage) {
//
//    SOCKET aiSock = socket(AF_INET, SOCK_STREAM, 0);
//    if (aiSock == INVALID_SOCKET) {
//        return R"({"status":"error","message":"AI 소켓 생성 실패"})";
//    }
//
//    sockaddr_in aiAddr{};
//    aiAddr.sin_family = AF_INET;
//    aiAddr.sin_port = htons(AI_PORT);
//    inet_pton(AF_INET, "127.0.0.1", &aiAddr.sin_addr);
//
//    if (connect(aiSock, (sockaddr*)&aiAddr, sizeof(aiAddr)) == SOCKET_ERROR) {
//        closesocket(aiSock);
//        return R"({"status":"error","message":"AI 서버 연결 실패"})";
//    }
//
//    // JSON 요청 + 개행(\n) → Python에서 readline()으로 안정적으로 받음
//    json req;
//    req["message"] = userMessage;
//    string reqStr = req.dump() + "\n";
//    send(aiSock, reqStr.c_str(), (int)reqStr.size(), 0);
//
//    // 응답 수신 (개행까지 읽기)
//    string totalResp;
//    char buffer[1024];
//    int len;
//    while ((len = recv(aiSock, buffer, sizeof(buffer) - 1, 0)) > 0) {
//        buffer[len] = '\0';
//        totalResp += buffer;
//        if (totalResp.find("\n") != string::npos) break;
//    }
//
//    closesocket(aiSock);
//    return totalResp;
//}
//
//// ========================== USER_TOTAL_RESULT 저장 ==========================
//json save_user_result(const json& data) {
//    json result;
//    MYSQL* conn = connect_db();
//    if (!conn) {
//        result["status"] = "fail";
//        result["message"] = "DB 연결 실패";
//        return result;
//    }
//
//    string u_id = data.value("U_ID", "");
//    string res_con = data.value("RES_CON", "");
//    string res_alive = data.value("RES_ALIVE", "");
//    string res_act = data.value("RES_ACT", "");
//
//    const char* query = "INSERT INTO USER_TOTAL_RESULT (U_ID, RES_CON, RES_ALIVE, RES_ACT) VALUES (?, ?, ?, ?)";
//    MYSQL_STMT* stmt = mysql_stmt_init(conn);
//    mysql_stmt_prepare(stmt, query, strlen(query));
//
//    MYSQL_BIND bind[4] = {};
//    bind[0].buffer_type = MYSQL_TYPE_STRING;
//    bind[0].buffer = (char*)u_id.c_str();
//    bind[0].buffer_length = u_id.length();
//
//    bind[1].buffer_type = MYSQL_TYPE_STRING;
//    bind[1].buffer = (char*)res_con.c_str();
//    bind[1].buffer_length = res_con.length();
//
//    bind[2].buffer_type = MYSQL_TYPE_STRING;
//    bind[2].buffer = (char*)res_alive.c_str();
//    bind[2].buffer_length = res_alive.length();
//
//    bind[3].buffer_type = MYSQL_TYPE_STRING;
//    bind[3].buffer = (char*)res_act.c_str();
//    bind[3].buffer_length = res_act.length();
//
//    mysql_stmt_bind_param(stmt, bind);
//
//    if (mysql_stmt_execute(stmt) == 0) {
//        result["status"] = "success";
//    }
//    else {
//        result["status"] = "fail";
//        result["message"] = mysql_error(conn);
//    }
//
//    mysql_stmt_close(stmt);
//    mysql_close(conn);
//    return result;
//}
//
//// ========================== USER_TOTAL_RESULT 조회 ==========================
//json handle_user_result(const json& data) {
//    json result;
//    MYSQL* conn = connect_db();
//    if (!conn) {
//        result["status"] = "fail";
//        result["message"] = "DB 연결 실패";
//        return result;
//    }
//
//    string u_id = data.value("U_ID", "");
//
//    const char* query = "SELECT RES_ID, RES_CON, RES_ALIVE, RES_ACT FROM USER_TOTAL_RESULT WHERE U_ID = ?";
//    MYSQL_STMT* stmt = mysql_stmt_init(conn);
//    mysql_stmt_prepare(stmt, query, strlen(query));
//
//    MYSQL_BIND bind[1] = {};
//    bind[0].buffer_type = MYSQL_TYPE_STRING;
//    bind[0].buffer = (char*)u_id.c_str();
//    bind[0].buffer_length = u_id.length();
//    mysql_stmt_bind_param(stmt, bind);
//    mysql_stmt_execute(stmt);
//
//    MYSQL_RES* meta = mysql_stmt_result_metadata(stmt);
//    int fieldCount = mysql_num_fields(meta);
//
//    // 결과 바인딩
//    MYSQL_BIND resultBind[4] = {};
//    int res_id;
//    char res_con[1000], res_alive[50], res_act[50];
//    unsigned long len0, len1, len2, len3;
//
//    resultBind[0].buffer_type = MYSQL_TYPE_LONG;
//    resultBind[0].buffer = &res_id;
//
//    resultBind[1].buffer_type = MYSQL_TYPE_STRING;
//    resultBind[1].buffer = res_con;
//    resultBind[1].buffer_length = sizeof(res_con);
//    resultBind[1].length = &len1;
//
//    resultBind[2].buffer_type = MYSQL_TYPE_STRING;
//    resultBind[2].buffer = res_alive;
//    resultBind[2].buffer_length = sizeof(res_alive);
//    resultBind[2].length = &len2;
//
//    resultBind[3].buffer_type = MYSQL_TYPE_STRING;
//    resultBind[3].buffer = res_act;
//    resultBind[3].buffer_length = sizeof(res_act);
//    resultBind[3].length = &len3;
//
//    mysql_stmt_bind_result(stmt, resultBind);
//
//    json rows = json::array();
//
//    while (mysql_stmt_fetch(stmt) == 0) {
//        json row;
//        row["RES_ID"] = res_id;
//        row["RES_CON"] = string(res_con, len1);
//        row["RES_ALIVE"] = string(res_alive, len2);
//        row["RES_ACT"] = string(res_act, len3);
//        rows.push_back(row);
//    }
//
//    result["status"] = "success";
//    result["data"] = rows;
//
//    mysql_free_result(meta);
//    mysql_stmt_close(stmt);
//    mysql_close(conn);
//    return result;
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
//        string action = request.value("action", "");
//        json response;
//
//        if (action == "login") {
//            response = handle_login(request["data"]);
//        }
//        else if (action == "ask_ai") {
//            response = json::parse(ask_ai_server(request.value("message", "")));
//        }
//        else if (action == "save_result") {
//            response = save_user_result(request["data"]);
//        }
//        else if (action == "get_result") {
//            response = handle_user_result(request["data"]);
//        }
//        else {
//            response["status"] = "fail";
//            response["message"] = "지원하지 않는 action";
//        }
//
//        string responseStr = response.dump();
//        send(clientSocket, responseStr.c_str(), (int)responseStr.size(), 0);
//    }
//    catch (const exception& e) {
//        cerr << "[에러] " << e.what() << endl;
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
//    sockaddr_in serverAddr{};
//    serverAddr.sin_family = AF_INET;
//    serverAddr.sin_port = htons(PORT);
//    serverAddr.sin_addr.s_addr = INADDR_ANY;
//
//    bind(listenSocket, (sockaddr*)&serverAddr, sizeof(serverAddr));
//    listen(listenSocket, SOMAXCONN);
//
//    cout << "[서버 시작] 포트 " << PORT << " 대기 중..." << endl;
//
//    while (true) {
//        SOCKET client = accept(listenSocket, NULL, NULL);
//        thread th(handleClient, client);
//        th.detach();
//    }
//
//    closesocket(listenSocket);
//    WSACleanup();
//}
