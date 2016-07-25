# import the necessary packages
from __future__ import print_function
import requests

def enum(**enums):
    return type('Enum', (), enums)
	
STATE = enum(IN_PLAY=0, DANGER_ZONE_RED=1, DANGER_ZONE_BLUE=2, OOF=3)
EVENT = enum(GOAL=0, START=1)

__DEBUG_PRINT__ = True
def debugPrint(txt):
	if __DEBUG_PRINT__:
		print(txt)

class Algorithm:
	Length = 5
	Width = 3
	NoneCountTH = 8
	possesionMatrix = []
	noneCount = 0

	def inArea(self, point, lowX, highX, lowY, highY):
		if (point[0]>lowX and point[0]<highX and point[1]>lowY and point[1]<highY):
			return True
		else:
			return False         
	
	def inCenter(self, point):
		return self.inArea(point, 0, 1000, 800, 1200)
					  
	def inRedDangerZone(self, point):
		return self.inArea(point, 200, 800, 0, 250)
		
	def inBlueDangerZone(self, point):
		return self.inArea(point, 200, 800, 1750, 2000)
	
	def HandleOOF(self, point):
		if (self.inCenter(point)):
			self.state = STATE.IN_PLAY
			sendEvent(EVENT.START, 1)
		debugPrint("End HandleOOF")
		
	def HandleDangerZone(self, point):
		if (noneCount == NoneCountTH):
			if (self.state == STATE.DANGER_ZONE_RED):
				sendEvent(EVENT.GOAL, 'BLUE')
			else:
				sendEvent(EVENT.GOAL, 'RED')
			self.state = STATE.OOF
		if not(self.inDangerZoneRed(point) or self.inDangerZoneBlue(point)):
			self.state = STATE.IN_PLAY
		debugPrint("End HandleDangerZone")

	def HandleInPlay(self, point):
		if (self.inDangerZoneRed(point)):
			self.state = STATE.DANGER_ZONE_RED
			redDangerZoneCount+=1
		if (self.inDangerZoneBlue(point)):
			self.state = STATE.DANGER_ZONE_BLUE
			blueDangerZoneCount+=1
		debugPrint("End HandleDangerZone")	

	def __init__(self):
		self.state = STATE.OOF
		redDangerZoneCount = 0
		blueDangerZoneCount = 0
		for i in range(self.Width):
			self.possesionMatrix.append([])
			for j in range(self.Length):
				self.possesionMatrix[i].append(0)

		self.debugFile = open("debug", "w")
		debugPrint("End main")
		
	def AddPoints(self, pointsArray):
		for point in pointsArray :
			lastState = self.state

			#############################################################

			if (point[0] == -1 and point[1] == -1):
				noneCount+=1
			else:
				noneCount = 0

			###### Call the relevant handler according the state ##############################
			if (self.state == STATE.IN_PLAY):
				self.HandleInPlay(point)
			if (self.state == STATE.DANGER_ZONE_RED or self.state == STATE.DANGER_ZONE_BLUE):
				self.HandleDangerZone(point)
			if (self.state == STATE.OOF):
				self.HandleOOF(point)

			self.debugFile.write("Point: " + str(point[0]) + "," + str(point[1]))
			debugPrint("Point: " + str(point[0]) + "," + str(point[1]))
			self.debugFile.write("state: " + str(self.state))
			debugPrint("state: " + str(self.state))


class GuiHttpClient(object):

	def SendEvent(self, action, team):
		r = requests.post('http://localhost/foosballApi/', data = action + '*' + team)
		if(r.status_code != requests.codes.ok):
			r.raise_for_status()

        def SendStats(self, redTeamAttacks, blueTeamAttacks):
                ebugPrint("End SendStats")

		
class EventHook(object):

	def __init__(self):
		self._Algo = Algorithm()

	def fire(self, *array):
		self._Algo.AddPoints(*array)

def main():
	x = EventHook()
	x.fire([[1,2], [999, 1999]])
	

if __name__ == "__main__":
    main()
	
	
