


separate segments from node dialog to segment dialog    OK

Trestle (bridge)    OK
    

node:
    - remove            OK
    - create new        OK
        node with 1 segment: in front       OK
        node with 2 segments: on side       OK
        node with 3 segments: disabled      OK
    - split node        
        track           OK
        switch          OK

segment
    - remove            OK
    - add span    
    - inject node       OK
       
    visualizer
        - show segments connected to node       OK
        - show segments by group id
           
telegraph pole 
    - move      OK
    - rotate    OK
    

track span visualizer

span dialog
    - lover, upper segment, distance

    
track node editor closed => State.SelectedAsset not set to null


track node remove with shift:  do not delete segment - connect it to other node