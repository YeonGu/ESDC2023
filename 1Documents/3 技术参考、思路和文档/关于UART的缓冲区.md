为了存放RFID模块传来的报文，并且为解析线程提供服务。报文解析线程通过`uint8_t UART_read_byte() `读取缓冲区的byte。

有两种方法：直接**用STL维护一个List（队列，FIFO）**；或者**手写一个环形缓冲区/List**。环形缓冲区的概念很直白，这里不再赘述。

思路大概是，在UART初始化后，**程序的一开始就开启一个接收中断**`
`HAL_UART_Receive_IT(&huart1, &rx_data, 1)`;
在接收到一个byte后就会进入callback function。此时**在Callback里再次开启中断**，并且**把数据push进队列里**。
```c
// 接收完成回调函数
void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
  if(huart->Instance == USART1)
  {
    // 处理接收到的数据
    // ...
    
    // 再次开启串口接收中断，继续接收下一个字节
    HAL_UART_Receive_IT(&huart1, &rx_data, 1);
  }
}
```

参考：
**最长情况下一次报文24byte**（24个uint8_t）。假设最坏情况下轮询64次的数据全部塞进去，占用1536byte。**1024次的数据占用不超过25kB**。
（对于RAM而言应该够用的，吧）