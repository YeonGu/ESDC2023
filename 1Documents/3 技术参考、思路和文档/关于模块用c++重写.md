好的，我可以尝试用C++把这段代码重写、封装一下，让它更加符合现代的编程风格。不过，我不能保证我的代码是完美的，您可能需要根据您的具体需求和环境进行一些修改和测试。

下面是我生成的C++代码示例：

```c++
// 使用code blocks语法
#include <iostream>
#include <vector>
#include <cstdint>

// 定义RFID帧类
class RFIDFrame {
private:
    uint8_t head;  // 帧头，固定为BB
    uint8_t addr;  // 地址，固定为02
    uint8_t cmd;   // 命令，固定为22
    uint16_t len;  // 数据长度，16位，最高支持0xFFFF
    std::vector<uint8_t> data; // 数据内容，卡号等；为一串byte
    uint8_t cs;    // 校验位，累加和的最低字节
    uint8_t tail;  // 帧尾，固定为7E

public:
    // 构造函数，初始化帧头、地址、命令和帧尾
    RFIDFrame() : head(0xBB), addr(0x02), cmd(0x22), tail(0x7E) {}

    // 读取一个报文帧，放进info里
    bool readFrame() {
        uint8_t byte;
        byte = UART_read_byte();

        // 没有遇到帧头，则继续读取直到遇到0xBB
        while (byte != head) {
            byte = UART_read_byte();
        }

        // 继续读取地址、命令、长度信息
        addr = UART_read_byte();
        cmd = UART_read_byte();
        len = (UART_read_byte() << 8) + UART_read_byte();

        data.resize(len); // 调整数据容器的大小
        for (int i = 0; i < len; i++) {
            data[i] = UART_read_byte();
        }

        // 继续读取校验位和帧尾，并存储到类成员中
        cs = UART_read_byte();
        tail = UART_read_byte();

        // 返回成功标志
        return true;
    }

    // 检查head、cmd、addr、校验位等是否正确
    bool checkFrame() {
        // 检查帧头、地址、命令和帧尾是否正确
        if (head != 0xBB || addr != 0x02 || cmd != 0x22 || tail != 0x7E) {
            return false; // 返回错误标志
        }

        // 计算校验位，从第二个字节到倒数第三个字节的累加和的最低字节
        unsigned char cs = addr + cmd + (len >> 8) + (len & 0xFF);
        for (int i = 0; i < len; i++) {
            cs += data[i];
        }
        cs &= 0xFF; // 取最低字节
        // 检查校验位是否正确
        if (this->cs != cs) {
            return false; // 返回错误标志
        }

        // 返回成功标志
        return true;
    }

    // 获取数据内容的方法
    std::vector<uint8_t> getData() const {
        return data;
    }
};

// 移植用的接口，返回一个UART缓冲区读取到的byte
uint8_t UART_read_byte() {
    return 0;
}

int main() {
    RFIDFrame frame; // 创建一个RFID帧对象

    if (frame.readFrame()) { // 如果读取成功
        if (frame.checkFrame()) { // 如果检查通过
            std::cout << "Valid frame received." << std::endl;
            std::cout << "Data: ";
            for (auto byte : frame.getData()) { // 遍历数据内容并打印出来
                std::cout << std::hex << byte << " ";
            }

