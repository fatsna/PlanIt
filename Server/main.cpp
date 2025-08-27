#include "server.h" //TCP 서버클래스 선언
#include <csignal>
#include <atomic>
#include <thread>
#include <iostream>

using namespace std;
atomic<bool>running_(true); // atomic :여러 스레드가 동시에 같은 변수에 접근해도 충돌하지않음

void signalHandler(int signum) {
    cout << "Stopping server.." << endl;
    running_ = false;
}

int main() { 

    signal(SIGINT, signalHandler); // Ctrl + C 종료

    //포트번호 12345인
    TcpServer server(12345);

    if (server.start()) {

        while (running_) {
            //단순히 프로그램이 끝나지 않게 무한 대기
            this_thread::sleep_for(chrono::seconds(1));
        }
        server.stop();
    }
    return 0;
}