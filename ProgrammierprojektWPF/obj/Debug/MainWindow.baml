   M S B A M L   `   `   `  ����P  LProgrammierprojektWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:ProgrammierprojektWPFProgrammierprojektWPF  5   	   &     ProgrammierprojektWPF.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:ProgrammierprojektWPF     5   	     � Title$  Client Menu��5   	   $	��675��6   $	��750��6*   $��0q�66   $��Bell MT)�6J   $��16(�6_   
���   � � WindowStartupLocation$ CenterScreen=�6�    � 
ResizeMode$ NoResize=�6�    � 	Resources 5
       �� 
TargetType �� Property �� Value �� 
TargetType �� RadiusX	 �� RadiusY
 �� Triggers P� Property P� Value��%z  '��       �� 5   
   " ��6(   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      �� #��z��6(   #��z��6L   $ 5��6r   $	 5��6~   �� 5      $��Center=�6/   $��Center=�6L   5      5      5      �� 5       � �� �	#FFC8C8C86+   
 5      P� 5       �@J�IsMouseOver . . �5      ��  � �� �	#FF98989863   5      P� 5       � �� . . �5      ��  � �� �	#FF68686863   5      P� 5       � {� .  . �5      ��  � �� �	#7FC8C8C863   �� 5        � �� �	#7F00000063   5!      5#      5$      ��5&      
� 5'   
   .��5(      �  � Color$ 	#FF121212��6    � Offset$ 0.233Q�6-   � 5)      $ 	#FF2F2E2E��6   $ 1Q�6-   5*      .��5-      � .+�5.   
   �� -   5         ��Name$ cmdConnectFour��5.      $��Play Connect Four (tbd)��6)   $��50,100,50,0q�6K   $	��Top=�6`   $��50��6x   $��40(�6�   {�.  6�   �� 5/   
   -   5        $ cmdChomp��5/      $��Play Chomp (tbd)��6#   $��50,200,50,0q�6>   $	��Top=�6S   $��50��6k   $��40(�6w   {�.  6�   �� 50   
   -   5        $ 	cmdServer��50      $��
Run Server��6$   $��50,300,50,0q�69   $	��Top=�6N   $��50��6f   $��40(�6r   6�   �� 51   
   -   5        $ 	cmdClient��51      $��
Run Client��6$   $��50,400,50,0q�69   $	��Top=�6N   $��50��6f   $��40(�6r   6�   �� 52   
   -   5        $ cmdQuit��52      $
��Quit��6"   $��50,500,50,0q�61   $	��Top=�6F   $��50��6^   $��40(�6j   6x   53      54      