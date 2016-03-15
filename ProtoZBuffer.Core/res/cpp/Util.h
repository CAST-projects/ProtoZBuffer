#pragma once
#include <google/protobuf/message.h>
#include <google/protobuf/io/zero_copy_stream.h>
#include <iostream>

%NAMESPACE_BEGIN%

    namespace Util
    {
        bool writeDelimited(google::protobuf::Message const& msg, std::ostream& zero, bool isRoot = false);
        bool readDelimited(std::istream& in, ::google::protobuf::Message& msg, bool isRoot = false);
    };

%NAMESPACE_END%
