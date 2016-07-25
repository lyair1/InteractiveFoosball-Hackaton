# import the necessary packages
from __future__ import print_function

def enum(**enums):
    return type('Enum', (), enums)
	
STATE = enum(IN_PLAY=0, NOT_IN_PLAY=1, DANGER_ZONE=2)

__DEBUG_PRINT__ = True
def debugPrint(txt):
	if __DEBUG_PRINT__:
		print(txt)

class Algorithm:
	Length = 5
	Width = 3
	possesionMatrix = []
	
	def __init__(self):
		for i in range(self.Width):
			self.possesionMatrix.append([])
			for j in range(self.Length):
				self.possesionMatrix[i].append(0)
				
		self.state = STATE.NOT_IN_PLAY
		
		debugPrint("End main")
		
	def AddPoints(self, array):
		self.possesionMatrix[array[0]][array[1]] +=1
		debugPrint("AddPoints")
		debugPrint(self.possesionMatrix)
		
class EventHook(object):

	def __init__(self):
		self._Algo = Algorithm()

	def fire(self, *array):
		self._Algo.AddPoints(*array)

def main():
	x = EventHook()
	x.fire([1,2])
	

if __name__ == "__main__":
    main()
	
	