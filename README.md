# DataToPictureTranslator
This software allows any data file to be saved in form of a picture.
The only public constructor lets you to designate the amount of
bytes of data to store per pixelSize, it also gives you the opportunity
to select size of a storage unit (pixelSize) as well as the height
and width of the bmp canvas.


# Save format description
This translator saves data by reading source's bytes and
representing them as a color hex values of a storage unit (a storage unit is a square pixelSize*pixelSize)
The first storage unit stores the 3-letter extention of the saved file,
in this example it is: r-106 g-112 b-103 => jpg 
![image](https://user-images.githubusercontent.com/66963865/234691340-8ea12320-18bc-43fc-94bc-0d3646b9f59e.png)

Following 8 storage units hold the amount of bytes encoded (not including the first 9)
These 8 only hold information in their first byte of color each.
The rest of the file is the encoded information.
The amount of bytes stored per storage unit can be specified in
the constructor, but has to lie within the range of 1 to 3.

# Data integrity
Decoded data is bytewise the same as the pre-encoded one
![image](https://user-images.githubusercontent.com/66963865/234697521-5277f1f9-cf1b-43a2-a41b-6da4aed38837.png)
