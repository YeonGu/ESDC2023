/**
 * @file    RFID_Module.cpp
 * @author  Gu Yuhang (213221544@seu.edu.cn)
 * @brief
    一个抽象的RFID_Module class。
    class RFIDReaderModule用于存储整体的信息，比如卡号的统计。
    class RFIDFrameInfo用于抽象和解析报文帧。

    我尽量做到整体代码的封装和可靠性，并且争取考虑所有的异常情况。

 * @version 0.1
 * @date 2023-04-10
 *
 * @copyright Copyright (c) 2023
 *
 */

#include <queue>
#include <stdint.h>
#include <vector>

class RFIDReaderModule {
  private:
};

/**
 * @brief RFID帧类。基本行为是通过一个 std::list<uint8_t>& _frameInfoBuffer 初始化。之后RFIDFrameInfo会读取缓冲区，解析报文信息。
 *
 */
class RFIDFrameInfo {
  private:
    // Frame Info
    uint8_t m_head;              // 帧头，固定为BB
    uint8_t m_addr;              // 地址，固定为02
    uint8_t m_cmd;               // 命令，固定为22
    uint16_t m_len = 0;          // 数据长度，16位，最高支持0xFFFF(25535)
    std::vector<uint8_t> m_data; // 数据内容，卡号等；为一串byte
    uint8_t m_cs;                // 校验位，累加和的最低字节
    uint8_t m_tail;              // 帧尾，固定为7E

    bool m_state = true;

    std::queue<uint8_t>& m_uart_buffer;

    uint8_t UartReadNextByte();
    bool RfidCheckFrame();

    bool RfidResolveFrame();

  public:
    // Constructors
    RFIDFrameInfo(std::queue<uint8_t>& buf) : m_uart_buffer(buf) {
        if (!RfidResolveFrame())
            m_state = false;
    }

    const std::vector<uint8_t> FrameInfo() const {
        return m_data;
    }
};

/**
 * @brief   RfidResolveFrame. 通过阻塞方式从头读取到帧尾。
            如果有异常，直接抛出false。
 *
 * @return true     所有格式正确
 * @return false    格式有问题。直接throw出去。
 */
bool RFIDFrameInfo::RfidResolveFrame() {
    bool normalFrame = true;

    // 检查报文头是否是0xBB
    if (UartReadNextByte() != 0xBB) {
        return false;
    }
    m_head = 0xBB;

    // 继续读取地址、命令、长度信息。做一步检查一步。
    m_addr = UartReadNextByte();
    if (m_addr == 0x7E)
        return false;

    m_cmd = UartReadNextByte();
    if (m_cmd == 0x7E)
        return false;

    m_len = UartReadNextByte();
    if (m_len == 0x7E)
        return false;
    m_len <<= 8;
    m_len += UartReadNextByte();
    if ((m_addr & 0xFF) == 0x7E)
        return false;

    // 读取卡号
    for (uint16_t i = 0; i < m_len; i++) {
        auto nextByte = UartReadNextByte();
        if (nextByte == 0x7E)
            return false;
        m_data.emplace_back(nextByte);
    }

    m_cs = UartReadNextByte();
    if (m_cmd == 0x7E)
        return false;

    m_tail = UartReadNextByte();
    while (m_tail != 0x7E) {
        normalFrame = false;
        m_tail = UartReadNextByte();
    }

    if (!RfidCheckFrame()) {
        return false;
    }

    return normalFrame;
}

/**
 * @brief   阻塞方式读取buffer的下一帧。如果buffer empty，就等待。
            主要是为了RfidResolveFrame提供接口。
 *
 * @return uint8_t
 */
uint8_t RFIDFrameInfo::UartReadNextByte() {
    while (m_uart_buffer.empty())
        ;
    uint8_t nextByte = m_uart_buffer.front();
    m_uart_buffer.pop();
    return nextByte;
}

bool RFIDFrameInfo::RfidCheckFrame() {
    // 检查帧头、地址、命令和帧尾是否正确
    if (m_head != 0xBB || m_addr != 0x02 || m_cmd != 0x22 || m_tail != 0x7E) {
        return false; // 返回错误标志
    }

    // 计算校验位，从第二个字节到倒数第三个字节的累加和的最低字节
    uint8_t cs = m_addr + m_cmd + (m_len >> 8) + (m_len & 0xFF);
    for (auto i : m_data) {
        cs += i;
    }

    return m_cs == cs;
}
