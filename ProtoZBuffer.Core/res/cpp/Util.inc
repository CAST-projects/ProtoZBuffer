#include <google/protobuf/message.h>
#include <google/protobuf/io/zero_copy_stream.h>
#include <iostream>
#include <fstream>
#include <sstream>
#include <google/protobuf/io/coded_stream.h>
#include <google/protobuf/io/zero_copy_stream_impl.h>

%NAMESPACE_BEGIN%

    namespace Util
    {
        bool writeDelimited(google::protobuf::Message const& msg, std::ostream& out, bool isRoot = false)
        {
            {
                google::protobuf::io::OstreamOutputStream zero(&out, 1);
                google::protobuf::io::CodedOutputStream coded(&zero);
                google::protobuf::uint32 msgSize = msg.ByteSize();
                coded.WriteVarint32(msgSize);
                if (!msg.SerializeToCodedStream(&coded))
                    return false;
                if (isRoot)
                    coded.WriteLittleEndian32(msgSize); // we read the root messages from the end
            } //forces flush

            return true;
        }

        bool readDelimited(std::istream& in, ::google::protobuf::Message& msg, bool isRoot = false)
        {
            google::protobuf::uint32 msgSize = 0;
            if (isRoot)
            {
                // google streams can only move forward, but we need to move backward for root messages
                // move 'in' stream before using google streams

                // find message size to position stream at message's beginning
                int size = sizeof(google::protobuf::uint32);
                int offset = -size;
                in.seekg(offset, std::ios::end);

                google::protobuf::io::IstreamInputStream zero(&in, 1);
                google::protobuf::io::CodedInputStream coded(&zero);
                if (!coded.ReadLittleEndian32(&msgSize))
                    return false;

                offset -= msgSize + google::protobuf::io::CodedOutputStream::VarintSize32(msgSize);
                in.seekg(offset, std::ios::end);
            }

            google::protobuf::io::IstreamInputStream zero(&in, 1);
            google::protobuf::io::CodedInputStream coded(&zero);
            if (!coded.ReadVarint32(&msgSize))
                return false;

            return msg.ParseFromBoundedZeroCopyStream(&zero, msgSize);
        }

};

%NAMESPACE_END%
