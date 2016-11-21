#include "catch.hpp"
#include <exception>
#include "FileStructure/Document.h"
#include "FileStructure/Data.h"
#include "FileStructure/Identity.h"

using namespace FileStructure;

TEST_CASE("TestInvalidNestedMessage")
{
	Document externalDoc;
	auto & externalIdentity = externalDoc.addData().addSecretIdentity();
	externalIdentity.setFirstName("toto");
	externalIdentity.setBirthYear(2013);

	Document currentDoc;
	auto & data = currentDoc.addData();
	CHECK_THROWS_AS(data.addReviewers(externalIdentity), std::runtime_error);
	CHECK_THROWS_AS(data.setApprover(externalIdentity), std::runtime_error);
}