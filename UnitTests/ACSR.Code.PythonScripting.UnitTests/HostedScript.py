class FormController:    
    def __init__(self, formInstance):
        self.FormInstance = formInstance
        
    def Customize(self):
        self.FormInstance.Text = "IronPython form"
        print "Here follows all the members in your form"
        #print dir(self.FormInstance)
        
    
def createFormControllerInstance(FormInstance):
    return FormController(FormInstance)
def test():
	return 10
#print CallIt()
#print "Hosted form caption: " + HostForm.Text