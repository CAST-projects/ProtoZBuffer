#include "catch.hpp"
#include <memory>
#include "FileStructure\Document.h"
#include "FileStructure\Data.h"
#include "FileStructure\Identity.h"
#include <fstream>
#include <filesystem>

using namespace std;
namespace fs = std::tr2::sys;

void checkData(fs::wpath const &outputFile)
{
    auto ifs = std::make_shared<std::fstream>(outputFile, std::ios_base::in | std::ios_base::binary);
    auto resultDoc = FileStructure::Document::ParseFrom(ifs);

    REQUIRE(resultDoc->dataCount() == 3);

    auto &d1 = resultDoc->getData(0);
    REQUIRE(d1.identification().firstName() == "UniverseAndEverything");
    REQUIRE(!d1.identification().hasBirthYear());
    REQUIRE(!d1.hasSecretIdentity());

    auto &d2 = resultDoc->getData(1);
    REQUIRE(d2.identification().firstName() == "Babbage");
    REQUIRE(d2.identification().birthYear() == 1791);
    REQUIRE(!d2.hasSecretIdentity());

    auto &d3 = resultDoc->getData(2);
    REQUIRE(d3.identification().firstName() == "Bruce");
    REQUIRE(d3.identification().birthYear() == 1964);
    REQUIRE(d3.hasSecretIdentity());
    REQUIRE(d3.secretIdentity()->firstName() == "Batman");
    REQUIRE(d3.secretIdentity()->birthYear() == 1989);
}

TEST_CASE("NestedData")
{
    auto doc = make_unique<FileStructure::Document>();
    {
        auto &d1 = doc->addData();
        auto &id1 = d1.identification();
        id1.setFirstName("UniverseAndEverything");

        auto &d2 = doc->addData();
        auto &id2 = d2.identification();
        id2.setBirthYear(1791);
        id2.setFirstName("Babbage");

        auto &d3 = doc->addData();
        auto &id3 = d3.identification();
        id3.setBirthYear(1964);
        id3.setFirstName("Bruce");
        auto &ids3 = d3.addSecretIdentity();
        ids3.setBirthYear(1989);
        ids3.setFirstName("Batman");
    }
    auto outputFile = fs::current_path<fs::wpath>() / fs::wpath(L"FileStructure.protozbuf");
    std::unique_ptr<FileStructure::Document> resultDoc;

    SECTION("Save and read")
    {
        fs::remove(outputFile);
        {
            std::ofstream ofs(outputFile, std::ios_base::binary);
            doc->writeDelimitedTo(ofs);
            ofs.close();
            REQUIRE(fs::exists(outputFile));
            REQUIRE(fs::file_size(outputFile) > 0);
        }
        checkData(outputFile);
    }
    SECTION("Build, save and read")
    {
        doc->build();
        REQUIRE(doc->getData(0).identification().firstName() == "UniverseAndEverything");
        fs::remove(outputFile);
        {
            std::ofstream ofs(outputFile, std::ios_base::binary);
            doc->writeDelimitedTo(ofs);
            ofs.close();
            REQUIRE(fs::exists(outputFile));
            REQUIRE(fs::file_size(outputFile) > 0);
        }
        checkData(outputFile);
    }



}


