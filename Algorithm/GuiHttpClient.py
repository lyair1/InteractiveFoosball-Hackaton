import requests

class GuiHttpClient(object):

	def SendHttp(self, action, team):
		r = requests.post('http://localhost/foosballApi/', data = action + '*' + team)
		if(r.status_code != requests.codes.ok):
			r.raise_for_status()
			
			
def main():
	x = GuiHttpClient()
	x.SendHttp("Miss","blue")
	

if __name__ == "__main__":
    main()