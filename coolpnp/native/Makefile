VIDEOSRC=$(wildcard video/*.cc)
VIDEOOBJ=$(addprefix bin/,$(notdir $(VIDEOSRC:.cc=.o)))
NATIVESRC=native.cc camera.cc vision.cc
NATIVEOBJ=$(addprefix bin/,$(NATIVESRC:.cc=.o)) $(VIDEOOBJ)
TESTSRC=test.cc
TESTOBJ=$(addprefix bin/,$(TESTSRC:.cc=.o))

CC=g++ -O6 -fPIC -fpermissive -c -I$(CURDIR) -I/usr/local/include/opencv -I/usr/local/include -std=c++11 -pthread $< -o $(addprefix bin/,$(notdir $@))
#CC=g++ -g -fPIC -c -I$(CURDIR) -std=c++11 -pthread $< -o $(addprefix bin/,$(notdir $@))

all: bin/native.so bin/test

bin/test: $(TESTOBJ) $(NATIVEOBJ)
	g++ $(TESTOBJ) -pthread -L/usr/local/lib -Lbin -lSDL2 -lSDL2main -lSDL2_ttf -lpthread -lopencv_imgproc -lopencv_core -lopencv_imgcodecs bin/native.so -o bin/test

bin/native.so: $(NATIVEOBJ)
	g++ $(NATIVEOBJ) -shared -pthread -L/usr/local/lib -Lbin -lSDL2 -lSDL2main -lSDL2_ttf -lpthread -lz -lopencv_imgproc -lopencv_core -lopencv_imgcodecs -o bin/native.so

$(TESTOBJ): *.h video/*.h

$(NATIVEOBJ): *.h video/*.h

bin/%.o: %.cc
	$(CC)

bin/%.o: video/%.cc
	$(CC)
