# InteractiveFoosball-Hackaton
Interactive Foosball project for the 2016 Microsoft Hackaton


Prepare your environment:

1. Download OpenCV(2.4.13): http://opencv.org/downloads.html
2. Install OpenCV and set up the environment variables: OPENCV_DIR <OpenCV_DIR>\Build\x86\vc12
3. Add "OPENCV_DIR <OPENCV_DIR>\Build\x86\vc12\bin" to your User and System Environment variables
4. Compile and run the project, you should see the demo video running (you might need to change the path to the file... you will see where)

Prepare your environment for Python:

1. Download OpenCV(2.4.13): http://opencv.org/downloads.html
2. Download and install Python 2.7 (DONT INSTALL PYTHON 3.0!!!!!)
3. Move the file: 
		C:\openCV\opencv\build\python\2.7\x64\cv2.pyd
	TO: 
		C:\Python27\Lib\site-packages
		
4. install Numpy from here: https://sourceforge.net/projects/numpy/files/NumPy/1.10.2/numpy-1.10.2-win32-superpack-python2.7.exe/download
5. Add Python to your PATH environment variable: C:\Python27\
6. Open the Commandline in administrator mode and go to: <REPO>\InteractiveFoosball-Hackaton\Python install\ and run 'python get-pip.py'
7. Add Pip path to your Path Environment Variable: C:\Python27\Scripts\pip
8. run in CMD: 'pip install imutils' 
6. Open the Commandline in administrator mode and go to: <REPO>\InteractiveFoosball-Hackaton\Foos-Python\ball-tracking\ball-tracking\ 
	and run: 'python ball_tracking.py'
	
	if everything worked fine you should see the webcamera



Checkout the POC here: https://www.youtube.com/watch?v=h8S8-cjuWZo
