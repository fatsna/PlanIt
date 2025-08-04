#pragma once         //중복정의로 인해 충돌됨을 방지함
// server.h가 프로젝트 여러 곳에서 #include되더라도 한 번만 포함되게 함.
#include <vector>    //클라이언트 스레드 저장용
#include <thread>   //멀티스레드
#include <mutex>    //동기화
#include <iostream>
#include <string>
//================== TCP 서버 클래스 ================
class TcpServer {
public:
    explicit TcpServer(int port);
    ~TcpServer();

    std::string sendToPythonAndReceive(const std::string& userMessage);
    bool start();
    void stop();


private:
    int server_fd_;         //서버소켓 파일디스크립터 **
    int python_fd_;         //Python AI서버와 연결된 별도의 소켓 **
    int port_;              //서버 포트 번호
    bool running_;          //서버 실행 상태 플래그
    std::string client_ip_; //마지막 접속한 클라이언트 IP 저장

    std::thread pythonReceiverThread_;       //Python 서버 수신전용 스레드 **
    std::vector<std::thread> clientThreads_; //클라이언트별 처리 스레드 목록 **  
    std::mutex threadMutex_;                 //스레드 동기화용 뮤텍스

    // ----------- 서버 소켓 준비 메서드 -----------
    bool createSocket();
    bool bindSocket();
    bool listenSocket();

    // ----------- 클라이언트 처리 메서드 -----------
    void acceptClients();               //클라이언트 연결 수락
    void handleClient(int client_fd);   //클라이언트 요청 처리
    void cleanup();                     //스레드 및 소켓 정리

    // ----------- Python AI 서버 연결 관련 -----------
    bool connectToPythonServer(const std::string& ip, int port); //Python AI 서버와 TCP연결시도
    void pythonReceiveThread();                                  //Python AI 서버에서 보내는 데이터를 수신하는 스레드  
    


    //void handlePythonProtocol7(const std::string& jsonStr);      //프로토콜 7(JSON) 처리(특정 JSON 프로토콜 처리)
                                                                   //메시지를 구분해서 알맞은 함수를 호출해서 처리하는 부분
};



