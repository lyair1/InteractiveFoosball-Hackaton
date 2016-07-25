# USAGE
# python ball_tracking.py --video ball_tracking_example.mp4
# python ball_tracking.py

# import the necessary packages
from __future__ import print_function
from collections import deque
from imutils.video import WebcamVideoStream
import numpy as np
import argparse
import imutils
import cv2
import time
import Algorithm
from Algorithm import EventHook

__ENABLE_VIDEO_OUT__ = True
__DEBUG_PRINT__ = False
__PRINT_VECTOR_TO_FILE = True

def getBallColor():
	lower = (0, 0, 145)
	upper = (179, 100, 255)

	return lower,upper

def debugPrint(txt):
	if __DEBUG_PRINT__:
		print(txt)

def savePtsVector(pts,ticks):
	if __PRINT_VECTOR_TO_FILE:
		txtFile = open("Output/Vector" + ticks + ".txt", "w")
		for (p1,p2) in pts:
			txtFile.write("{0}	{1}".format(p1,p2) + "\n")
		txtFile.close()

def main():
	points = []
	ticks =str(int(time.time() * 1000))
	# construct the argument parse and parse the arguments
	ap = argparse.ArgumentParser()
	ap.add_argument("-v", "--video",
		help="path to the (optional) video file")
	ap.add_argument("-b", "--buffer", type=int, default=64,
		help="max buffer size")
	args = vars(ap.parse_args())

	# define the lower and upper boundaries of the "green"
	# ball in the HSV color space, then initialize the
	# list of tracked points

	# BLUE
	ballLower, ballUpper = getBallColor()

	pts = deque(maxlen=args["buffer"])

	# created a *threaded *video stream, allow the camera senor to warmup,
	# and start the FPS counter
	debugPrint("[INFO] sampling THREADED frames from webcam...")
	vs = WebcamVideoStream(src=1).start()
	frame = vs.read()
	if frame is None:
		debugPrint("Changed to default camera!")
		# Take the default camera (webcam is not connected)
		vs = WebcamVideoStream(src=0).start()

	if __ENABLE_VIDEO_OUT__:
		outStream = cv2.VideoWriter('Output/output' + ticks +'.avi', -1, 20.0, (640,425))

	eventHook = EventHook()

	# keep looping
	while True:
		# grab the current frame
		frame = vs.read()

		# resize the frame, blur it, and convert it to the HSV
		# color space
		frame = imutils.resize(frame, width=640)
		frame = frame[45:470,0:640]

		# blurred = cv2.GaussianBlur(frame, (11, 11), 0)
		hsv = cv2.cvtColor(frame, cv2.COLOR_BGR2HSV)

		# construct a mask for the color "green", then perform
		# a series of dilations and erosions to remove any small
		# blobs left in the mask
		mask = cv2.inRange(hsv, ballLower, ballUpper)
		mask = cv2.erode(mask, None, iterations=2)
		mask = cv2.dilate(mask, None, iterations=2)

		# find contours in the mask and initialize the current
		# (x, y) center of the ball
		cnts = cv2.findContours(mask.copy(), cv2.RETR_EXTERNAL,
			cv2.CHAIN_APPROX_SIMPLE)[-2]
		center = None

		# only proceed if at least one contour was found
		if len(cnts) > 0:
			# find the largest contour in the mask, then use
			# it to compute the minimum enclosing circle and
			# centroid
			c = max(cnts, key=cv2.contourArea)
			((x, y), radius) = cv2.minEnclosingCircle(c)
			M = cv2.moments(c)
			center = (int(M["m10"] / M["m00"]), int(M["m01"] / M["m00"]))

			# only proceed if the radius meets a minimum size
			if radius > 2: #Changed from 10
				# draw the circle and centroid on the frame,
				# then update the list of tracked points
				cv2.circle(frame, (int(x), int(y)), int(radius),
					(0, 255, 255), 2)
				cv2.circle(frame, center, 5, (0, 0, 255), -1)

		# update the points queue
		pts.appendleft(center)

		debugPrint(center)
		if center is None:
			points.append((-1,-1))
		else:
			points.append(center)

		# loop over the set of tracked points
		for i in xrange(1, len(pts)):
			# if either of the tracked points are None, ignore
			# them
			if pts[i - 1] is None or pts[i] is None:
				continue

			# otherwise, compute the thickness of the line and
			# draw the connecting lines
			thickness = int(np.sqrt(args["buffer"] / float(i + 1)) * 2.5)
			cv2.line(frame, pts[i - 1], pts[i], (0, 0, 255), thickness)

		# show the frame to our screen
		cv2.imshow("Frame", frame)
		
		if __ENABLE_VIDEO_OUT__:
			outStream.write(frame)
		
		key = cv2.waitKey(1) & 0xFF

		# if the 'q' key is pressed, stop the loop
		if key == ord("q"):
			break

		if center is None:
			eventHook.fire([(-1,-1)])
		else:
			normalizePoint = (int(2000*float(center[0]/640.0)),int(1000*float(center[1]/420.0)))
			eventHook.fire([normalizePoint])
			debugPrint(normalizePoint)

	# cleanup the camera and close any open windows
	cv2.destroyAllWindows()
	vs.stop()
	savePtsVector(points,ticks)
	raise SystemExit
	if __ENABLE_VIDEO_OUT__:
		outStream.release()



if __name__ == "__main__":
    main()