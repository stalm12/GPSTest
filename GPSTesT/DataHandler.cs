﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace GPSTesT
{
	 class DataHandler
	 {
		  private Queue<string> data;			  //Contains all received data
		  private Queue<string> displayData;  //Contains not yet displayed data
		  private Queue<string> input;		  //Contains received not yet processec data
		  private Queue<string> ggaQueue;		  //Contains all GGA messages
          private string fileLoc;
		  private string actualLine;			  //Value of actual line
		  private bool first;					  //Indicates functions first call
		  private string filterString;
          private StreamWriter writer;
          private int count;
          private int divisor;

		  public DataHandler(Queue<string> displayData, Queue<string> input, Queue<string>ggaQueue)		  //Constructor
		  {
				this.displayData = displayData;
				this.ggaQueue = ggaQueue;
				this.input = input;
				data = new Queue<string>();
				first = true;
				actualLine = "";
				filterString = "";
                count = 30;
                divisor = 30;
		  }

          private void changeDivisor(int divisor)
          {
              this.divisor = divisor;
          }

		  public void Add()		  //Process data
		  {
				string Line, replaced;
                char[] splitChar = { ',', ' ' };
                string[] splittedString;
                splittedString = filterString.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);  //Get all filter values
                writer = File.AppendText(fileLoc);
				if (first == false)			 //If not first call
				{
					 while (input.Count > 0) //While not all data processed
					 {
						  Line = input.Dequeue();						//Get line
						  replaced = Line.Replace('\x24', '\n');	//Replace $
						  data.Enqueue(replaced);						//Save line
						  if (filterString == "")
						  {
								displayData.Enqueue(replaced);				//Hand line to display
						  }
						  else
						  {
                              for (int b = 0; b < splittedString.Length; b++)   //Test for every filter entry
                              {
                                  if ((replaced.Contains(splittedString[b]) == true))   //If message contains filter sequence
                                  {
                                      if (count == 0)                           //If divisor says save
                                      {
                                          displayData.Enqueue(replaced);    //Hand line to display
                                          writer.WriteLine(replaced);       //Save line to file
                                      }
                                  }
                              }
						  }
						  if ((Line.Contains("GPGGA") == true))             //If line is a GGA message
						  {
                              if (count == 0)                               //If divisor says save
                              {
                                  ggaQueue.Enqueue(replaced);               //Save message
                                  count = divisor;                          //Reset count
                              }
                              count--;                                      //Decrease divisor counter
						  }
					 }
				}
				else
				{
					 input.Dequeue();		//Discard first values
					 first = false;
				}
                writer.Close();
		  }

		  public string Get(int position)	  //Return a line
		  {
				if (position < data.Count)
				{
					 return data.ElementAt(position);
				}
				else
				{
					 return "";
				}
		  }


		  public ArrayList GetFilteredStatic(string filterString)	//Filter all data
		  {
				ArrayList result = new ArrayList();
                char[] splitChar = {',', ' '};
				string temp;
                string[] splittedString;
                splittedString = filterString.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
				for (int a = 0; a < data.Count; a++)				//All data
				{
					 temp = data.ElementAt(a);							//Get line
					 if (filterString != "")
					 {
						  for (int b = 0; b < splittedString.Length; b++)
						  {
								if (temp.Contains(splittedString[b]) == true)		//Check if line contains filter sequence
								{
									 result.Add(temp);
								}

						  }
					 }
					 else
					 {
						  result.Add(temp);
					 }
				}
				return result;
		  }

		  public void GetFiltered(string filterString)	//Filter all data
		  {
                char[] splitChar = { ',' };
                string[] splittedString;
                splittedString = filterString.Split(splitChar);
				ArrayList existingItems = GetFilteredStatic(filterString);
				this.filterString = filterString;

				for (int a = 0; a < existingItems.Count; a++)
				{
					 displayData.Enqueue((string)existingItems[a]);
				}
		  }

          public void SetFileName(string fileName)
          {
              fileLoc = fileName;
          }

          public void CloseFile()
          {
              writer.Close();
          }

	 }
}
