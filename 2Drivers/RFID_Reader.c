/**
 * @file    RFID_Reader.c
 * @author  Gu Yuhang (213221544@seu.edu.cn)
 * @brief
 * @version 0.1
 * @date    2023-04-05
 *
 * @copyright Copyright (c) 2023, ST年度峰会视察小队
 *
 *  返回报文实例（8位长度卡号）
 *  BB  02  22      00 09   CF    10 00   03 26 92 01     2A 79   6B   7E
 *  帧头 地址 命令    长度     RSSI  PC      卡号(EPC)        CRC    校验  停止位
 */

#include "RFID_Reader.h"
#include <stdint.h>
#include <vcruntime.h>

/**
 * @brief frame_t 存放报文字段中的数据
 *
 */
typedef struct {
    uint8_t head;  // 帧头，固定为BB
    uint8_t addr;  // 地址，固定为02
    uint8_t cmd;   // 命令，固定为22
    uint16_t len;  // 数据长度，16位，最高支持0xFFFF
    uint8_t* data; // 数据内容，卡号等；为一串
    uint8_t cs;    // 校验位，累加和的最低字节
    uint8_t tail;  // 帧尾，固定为7E
} RFID_frame_t;

// 移植用的接口
uint8_t UART_read_byte() {
    return 0;
}

/**
 * @brief
 *
 * @param info
 * @return int
 */
int read_frame(RFID_frame_t* info) {
    uint8_t byte;
    byte = UART_read_byte();
    // 没有遇到帧头，则继续读取直到遇到0xBB
    while (byte != 0xBB) {
        byte = UART_read_byte();
    }
    info->head = byte;
    // 继续读取地址、命令、长度信息
    info->addr = UART_read_byte();
    info->cmd = UART_read_byte();
    info->len = (UART_read_byte() << 8) + UART_read_byte();

    info->data = malloc(info->len);

    return RFID_OK;
}