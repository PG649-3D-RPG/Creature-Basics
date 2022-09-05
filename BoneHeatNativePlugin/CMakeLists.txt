cmake_minimum_required(VERSION 3.3)
project(BoneHeatNativePlugin)

# set source code directory
set(SourceDir src)

# find all source files
file(GLOB_RECURSE SRC_FILES 
	${SourceDir}/*.cpp
	${SourceDir}/*.h
)

include_directories (${SourceDir})

# create a shared library (.dll/.so)
add_library(BoneHeat SHARED ${SRC_FILES})

