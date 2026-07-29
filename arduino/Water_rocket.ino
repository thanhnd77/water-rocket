#include <NewPing.h>

// Chân TX,RX trên Module Bluetooth HC-05 cắm lần lượt vào chân RX,TX trên Board Arduino
int ledPin = 13;
int Wsensor = 2; // cảm biến lưu lượng
int Rsensor = 10; // cảm biến tên lửa., 1 : kho6ng, 0 co1

int Wpump = 3 ;  // bơm nước   Lam
int CC = 4;  // Căng cáp hãm  Đen
int P = 5; // Khí nén   Trắng
int C = 6;// Kẹp  Vàng // Claim  Kẹp 

String readString;String M;
int Wlevel = 0; // lượng nước định trước
long count = 0; long count1;long count2;
int T=0;

String inString = ""; 
 
// Laser TFmini
#include <SoftwareSerial.h>  //header file of software serial port

SoftwareSerial Serial1(8,9); //define software serial port name as Serial1 and define pin2 as RX and pin3 as TX
// chỉ cần dùng chân Tx (xanh lá) của TFmini=> Rx 8 của arduino
int dist;  //actual distance measurements of LiDAR
//int strength; //signal strength of LiDAR
//int check;  //save check value
//int i;
//int uart[9];  //save data measured by LiDAR
//const int HEADER=0x59;  //frame header of data package


void setup() {
  Serial.begin(57600);
    //Serial.begin(115200);
  pinMode(ledPin, OUTPUT);
  pinMode(Wpump, OUTPUT);
  pinMode(P, OUTPUT);
  pinMode(CC, OUTPUT);
  pinMode(C, OUTPUT);
  pinMode(Wsensor,INPUT_PULLUP);
  
  Serial1.begin(57600); //set bit rate of serial port connecting LiDAR with Arduino
  
}

void loop() 
{
  //Serial.println("Ready");
  // nhận thông tin từ cổng com
  while (Serial.available()) {
    delay(3);  
    char c = Serial.read();
   readString += c; 
  }
  if (readString.length() >0) 
  {
    if (readString.startsWith("W"))   // Định lượng nước bơm vào 
    { 
      readString.replace('W', '0');
      Wlevel = readString.toInt();
      Serial.println(Wlevel);
      digitalWrite(ledPin, HIGH);
     }
      
    if (readString.startsWith("N"))   // Mở bơm nước  
    {
      digitalWrite(C, HIGH);// kẹp tên lửa.
     // Serial.println("Water");
      //while (digitalRead(Rsensor)){}; // chờ cho đến khi tên lửa được kẹp xong.
      digitalWrite(Wpump, HIGH); // mở bơm
      count =0; count1=0;count2=0;
      Serial.println(count1);
      while(Wlevel > count1)         // trong khi chờ nước bơm đủ, đọc và gửi thông tin lượng nước đã bơm 
      {
        if(T != digitalRead(Wsensor))
          {
            count++; T= digitalRead(Wsensor);
            count1= int(count/90); //count2 = 10* count1; // 1/64.7
           // if ((count1 % 5 == 0 )&&(count % 5 == 0 )) // cứ 5% mới báo 1 lần
		 if ((count1!=0)&&(count1 *90 == count))// && (count1%5 ==0))
			{ Serial.print(count1);}
		}
      }
      count =0;
      digitalWrite(Wpump, LOW); // tắt bơm nước
      delay(200);
      digitalWrite(P, HIGH); // mở khí nén
      delay(2000); // chờ cho đủ áp
      Serial.println('R');// báo nước và áp lực đã sẵn sàng
     }
     if (readString.startsWith("F") )  //  bắn 
    {
      digitalWrite(C, LOW);// mở kẹp
      delay(50);
      digitalWrite(P, LOW);// tắt khí nén.
      //delay(50);
      digitalWrite(CC, HIGH);// mở căng cáp.
      Serial.println("D");// Done , báo đã bắn xong
      // Bắt đầu đo khoảng cách trong 3 sec
      long Time = millis();
      long Dis = 500;
       while ((millis()- Time)< 3000) {
        // đọc dữ liệu độ cao từ arduino 2 qua cổng Serial1
           while (Serial1.available() > 0) {
            int inChar = Serial1.read();
            if (isDigit(inChar)) {
            // convert the incoming byte to a char and add it to the string:
            inString += (char)inChar;
          }
          // if you get a newline, print the string, then the string's value:
      if (inChar == '\n') {
      dist = inString.toInt();
      inString = "";
      }
           }
            
            
            if ((dist >0)&& (dist < Dis))// đo lấy kc ngắn nhất.
             {Dis = dist;}
                    
        }
          
       Serial.println(500-Dis);// xuất giá trị độ cao.
       delay(1500);
       digitalWrite(CC, LOW);// tắt căng cáp.                        
    } 
    }
    
    readString="";
    
  
} 
/*void TFmini()
{
if (Serial1.available()) {  //check if serial port has data input
    if(Serial1.read() == HEADER) {  //assess data package frame header 0x59
      uart[0]=HEADER;
      if (Serial1.read() == HEADER) { //assess data package frame header 0x59
        uart[1] = HEADER;
        for (i = 2; i < 9; i++) { //save data in array
          uart[i] = Serial1.read();
        }
        check = uart[0] + uart[1] + uart[2] + uart[3] + uart[4] + uart[5] + uart[6] + uart[7];
        if (uart[8] == (check & 0xff)){ //verify the received data as per protocol
          dist = uart[2] + uart[3] * 256;     //calculate distance value
          strength = uart[4] + uart[5] * 256; //calculate signal strength value
        }
      }
    }
  }
}  */
