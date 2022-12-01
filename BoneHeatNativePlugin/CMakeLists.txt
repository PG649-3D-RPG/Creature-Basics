cmake_minimum_required(VERSION 3.16.3)
project(
	BoneHeatNativePlugin
	VERSION 0.1)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# set source code directory
set(SourceDir src)

# find all source files
file(GLOB_RECURSE SRC_FILES 
	${SourceDir}/*.cpp
	${SourceDir}/*.h
)

include_directories (${SourceDir})

set(PMP_SOURCE_DIR "external/pmp-library")

set(PMP_BUILD_EXAMPLES OFF)
set(PMP_BUILD_TESTS OFF)
set(PMP_BUILD_DOCS OFF)
set(PMP_BUILD_VIS OFF)
set(PMP_INSTALL OFF)
set(PROJECT_BINARY_DIR ${CMAKE_CURRENT_BINARY_DIR})
set(LIBRARY_OUTPUT_PATH ${PROJECT_BINARY_DIR})
set(EXECUTABLE_OUTPUT_PATH ${PROJECT_BINARY_DIR})

include_directories(${PMP_SOURCE_DIR}/src)
add_subdirectory(${PMP_SOURCE_DIR})

set(EIGEN_SOURCE_DIR ${PMP_SOURCE_DIR}/external/eigen)
include_directories(${EIGEN_SOURCE_DIR})

# create a shared library (.dll/.so)
add_library(BoneHeat SHARED ${SRC_FILES})

target_link_libraries(BoneHeat pmp)