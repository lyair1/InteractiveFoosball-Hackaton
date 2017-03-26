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
		
4. Add Python to your PATH environment variable: C:\Python27\
5. Open the Commandline in administrator mode and go to: <REPO>\InteractiveFoosball-Hackaton\Python install\ and run 'python get-pip.py'
6. Add Pip path to your Path Environment Variable: C:\Python27\Scripts\
7. run in CMD: 'pip install imutils' 

8. Open the CMD and run: 'pip install <REPO>\InteractiveFoosball-Hackaton\Python install\numpy-1.11.1+mkl-cp27-cp27m-win_amd64.whl'

9. Open the Commandline in administrator mode and go to: <REPO>\InteractiveFoosball-Hackaton\Foos-Python\ball-tracking\ball-tracking\ 
	and run: 'python ball_tracking.py'
	
	if everything worked fine you should see the webcamera



Checkout the POC here: https://www.youtube.com/watch?v=h8S8-cjuWZo


And also... The final result from Microsoft's hackathon in Israel: https://www.youtube.com/watch?v=GB3ThmAb_h8&t=5s

