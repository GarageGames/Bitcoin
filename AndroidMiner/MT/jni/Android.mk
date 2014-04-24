LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_LDLIBS	:=-llog
LOCAL_MODULE    := MT
LOCAL_SRC_FILES := MT.cpp F2M_MinerConnection.cpp F2M_Sockets_posix.cpp F2M_Net_posix.cpp

include $(BUILD_SHARED_LIBRARY)
