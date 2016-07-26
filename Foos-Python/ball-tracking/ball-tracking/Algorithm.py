# import the necessary packages
from __future__ import print_function
import requests

def enum(**enums):
    return type('Enum', (), enums)
	
STATE = enum(IN_PLAY=0, DANGER_ZONE_RED=1, DANGER_ZONE_BLUE=2, OOF=3)

EVENT = enum(Goal=0, Start=1, Miss=2)

__DEBUG_PRINT__ = True
def debugPrint(txt):
	if __DEBUG_PRINT__:
		print(txt)

class GuiHttpClient(object):
	def SendEvent(self, action, team):
		event = ""
		if(action == 0):
			event = "Goal"
		if(action == 1):
			event = "Start"
		if(action == 2):
			event = "Miss"
		# r = requests.post('http://localhost/foosballApi/', data = event + '*' + team)
		# if(r.status_code != requests.codes.ok):
			# r.raise_for_status()
		debugPrint("event + '*' + team")

		debugPrint("Send event")

	def SendPossession(self, blue, center, red):
		# r = requests.post('http://localhost/foosballApi/', data = "Possession" + '*' + blue + '*' + center+ '*' + red)
		# if(r.status_code != requests.codes.ok):
			# r.raise_for_status()
		debugPrint("Possession" + '*' + str(blue) + '*' + str(center)+ '*' + str(red))

		debugPrint("End SendPossessions")

	def sendHotSpots(self, possessionMatrix):
		# r = requests.post('http://localhost/foosballApi/', data = "HotSpots" + '*' + possessionMatrix)
		# if(r.status_code != requests.codes.ok):
			# r.raise_for_status()
		debugPrint("HotSpots" + '*' + str(possessionMatrix[0][0]))

		debugPrint("End SendHotSpot")

class Algorithm:
	NoneCountTH = 35
	Length = 1000
	Width = 2000
	
	NoneCountTH = 20
	MissPointCountTH = 15
	inGoalZone = False
	enteredGoalZonePoint = -50
	
	possessionMatrix = []
	noneCount = 0
	pointsCount = 0
	httpClient = GuiHttpClient()
	redDangerZoneCount = 0
	blueDangerZoneCount = 0
	centerZoneCount = 0
	noise = 0

	def inArea(self, point, lowX, highX, lowY, highY):
		if (point[0]>lowX and point[0]<highX and point[1]>lowY and point[1]<highY):
			return True
		else:
			return False         
	def isNone(self, point):
		if (point[0] == -1 and point[1] == -1):
			return True

	def inCenter(self, point):
		return self.inArea(point, 700, 1300, 0, 1000)
					  
	def inRedDangerZone(self, point):
		return self.inArea(point, 0, 700, 200, 800)
		
	def inBlueDangerZone(self, point):
		return self.inArea(point, 1300, 2000, 200, 800)
		
	def inRedGoalZone(self, point):
		return self.inArea(point, 0, 200, 250, 750)
		
	def inBlueGoalZone(self, point):
		return self.inArea(point, 1800, 2000, 250, 750)

	def HandleOOF(self, point):
		if (self.inCenter(point)):
			self.state = STATE.IN_PLAY
			self.debugFile.write("Game Started \n")
			self.httpClient.SendEvent(EVENT.Start, 'Blue')
		debugPrint("End HandleOOF")
		
	def HandleDangerZone(self, point):
		if (self.noneCount == self.NoneCountTH):
			if (self.state == STATE.DANGER_ZONE_RED):
				self.httpClient.SendEvent(EVENT.Goal, 'Blue')
				self.debugFile.write("Blue Goal\n")
			else:
				self.httpClient.SendEvent(EVENT.Goal, 'Red')
				self.debugFile.write("Red Goal\n")
			self.state = STATE.OOF
		if not(self.inRedDangerZone(point) or self.inBlueDangerZone(point) or self.isNone(point)):
			self.state = STATE.IN_PLAY
		debugPrint("End HandleDangerZone")

	def HandleInPlay(self, point):
		if (self.inRedDangerZone(point)):
			self.state = STATE.DANGER_ZONE_RED
		if (self.inBlueDangerZone(point)):
			self.state = STATE.DANGER_ZONE_BLUE
		debugPrint("End HandleInPlay")	
		
	def IncreasePossession(self, point):
		if(self.inRedDangerZone(point)):
			self.redDangerZoneCount+=1
		elif(self.inBlueDangerZone(point)):
			self.blueDangerZoneCount+=1
		elif(self.inCenter(point)):
			self.centerZoneCount+=1
		

		sumPossession = self.redDangerZoneCount + self.blueDangerZoneCount + self.centerZoneCount
		if(sumPossession % 10 == 1):
			self.httpClient.SendPossession(self.blueDangerZoneCount/float(sumPossession), self.centerZoneCount/float(sumPossession), self.redDangerZoneCount/float(sumPossession))
			
		if(sumPossession % 500 == 0):
			self.httpClient.sendHotSpots(self.possessionMatrix)
			
	# handling miss by detecting ball entering and leaving goal zone
	def HandleMiss(self, point):
		if(self.inGoalZone):
			if((not self.inBlueGoalZone(point)) and not (self.inRedGoalZone(point)) and not(self.isNone(point))):
				self.inGoalZone = False
				self.debugFile.write("leaved goal zone")
				if(self.enteredGoalZonePoint + self.MissPointCountTH >= self.pointsCount):
					self.httpClient.SendEvent(EVENT.Miss, '')
					self.debugFile.write("Sending MISS")
		else:
			if(self.inBlueGoalZone(point) or self.inRedGoalZone(point)):
				self.inGoalZone = True
				self.debugFile.write("wntered goal zone")
				self.enteredGoalZonePoint = self.pointsCount

			
	def __init__(self):
		self.state = STATE.OOF
		self.redDangerZoneCount = 0
		self.blueDangerZoneCount = 0
		
		for i in range(self.Width / 100):
			self.possessionMatrix.append([])
			for j in range(self.Length / 100):
				self.possessionMatrix[i].append(0)


		self.debugFile = open("debug.txt", "w")
		debugPrint("End main")
		
	def AddPoints(self, pointsArray):
		for point in pointsArray :
			lastState = self.state
			self.pointsCount += 1 
			self.IncreasePossession(point)
			self.HandleMiss(point)
			#############################################################

			if (point[0] == -1 and point[1] == -1):
				self.noneCount += 1
			else:
				self.possessionMatrix[point[0]/100][point[1]/100] += 1
				if self.noneCount > 4 and self.noise < 10 :
					self.noise+=1
				else:
					self.noneCount = 0
					self.noise = 0

			###### Call the relevant handler according the state ##############################
			if (self.state == STATE.IN_PLAY):
				self.HandleInPlay(point)
			if (self.state == STATE.DANGER_ZONE_RED or self.state == STATE.DANGER_ZONE_BLUE):
				self.HandleDangerZone(point)
			if (self.state == STATE.OOF):
				self.HandleOOF(point)

			self.debugFile.write("Point: " + str(point[0]) + "," + str(point[1]) + "\n")
			debugPrint("Point: " + str(point[0]) + "," + str(point[1]))
			if lastState != self.state :
				self.debugFile.write("state changed to: " + str(self.state) + "\n")
				debugPrint("state changed to: " + str(self.state))
		
class EventHook(object):
	def __init__(self):
		self._Algo = Algorithm()

	def fire(self, *array):
		self._Algo.AddPoints(*array)

def main():
	y = EventHook()
	
	with open("D:\hackathon2016\Algorithm\Data\Vector1469369882184.txt") as f:
		content = f.readlines()
	content2 = [[int(x) for x in thing.replace('\n', '').split('\t')] for thing in content]
	# print(content2)
	y.fire(content2)
	

if __name__ == "__main__":
    main()
	
	
