
import clr
clr.AddReference("System.Windows.Forms")
from System.Windows.Forms import *
from System.IO import *
import System

class FormController:    
    def __init__(self, formInstance):
        self.FormInstance = formInstance
        
    def Customize(self):
        self.FormInstance.Text = "IronPython form"
        print "Here follows all the members in your form"
        #print dir(self.FormInstance)
        
    
def createFormControllerInstance(FormInstance):
    return FormController(FormInstance)

def test(add): 
    return add + globalAdd



def TestByRef(ALambda, refInt ):
    #ALambda()
    MessageBox.Show(repr(dir(refInt)))
    refInt.value = 10

print "Script executed"