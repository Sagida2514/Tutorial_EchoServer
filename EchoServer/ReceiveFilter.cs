using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketEngine.Protocol;
using System;

namespace EchoServer
{
    //네트워크를 통해 수신된 바이너리 데이터의 구조를 정의합니다.
    public class EFBinaryRequestInfo : BinaryRequestInfo
    {
        public Int16 TotalSize { get; private set; } // 패킷의 총 크기를 나타냅니다.
        public Int16 PacketID { get; private set; } // 패킷 식별자입니다.
        public SByte Value1 { get; private set; } // 추가 데이터를 나타내는데 사용할 수 있는 바이트입니다.

        public const int HEADER_SIZE = 5; // 헤더의 크기를 나타내는 상수입니다.

        public EFBinaryRequestInfo(Int16 totalSize, Int16 packetID, SByte value1, byte[] body)
            : base(null, body)
        {
            this.TotalSize = totalSize;
            this.PacketID = packetID;
            this.Value1 = value1;
        }

    }
    //네트워크로부터 고정된 헤더 크기를 갖는 데이터를 수신하는 필터를 정의합니다.
    public class ReceiveFilter : FixedHeaderReceiveFilter<EFBinaryRequestInfo>
    {
        public ReceiveFilter() : base(EFBinaryRequestInfo.HEADER_SIZE)
        {

        }

        //헤더에서 몸체의 길이를 추출하는 메서드입니다.
        //여기서 리틀 엔디안 포맷의 데이터를 올바르게 처리하기 위해 시스템의 엔디안과 다를 경우
        //바이트 배열을 뒤집는 로직이 포함되어 있습니다.
        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header, offset, 2);

            var packetTotalSize = BitConverter.ToInt16(header, offset);
            return packetTotalSize - EFBinaryRequestInfo.HEADER_SIZE;
        }

        //수신된 데이터로부터 EFBinaryRequestInfo 객체를 생성하는 메서드입니다.
        //이 메서드는 헤더 정보를 해석하여 EFBinaryRequestInfo 인스턴스를 생성하고,
        //그 인스턴스에 수신된 본문 데이터를 전달합니다.

        protected override EFBinaryRequestInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(header.Array, 0, EFBinaryRequestInfo.HEADER_SIZE);

            return new EFBinaryRequestInfo(BitConverter.ToInt16(header.Array, 0), 
                BitConverter.ToInt16(header.Array, 0 + 2),
                (SByte)header.Array[4],
                bodyBuffer.CloneRange(offset, length));
        }
    }
}
