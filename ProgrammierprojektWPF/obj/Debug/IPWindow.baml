   M S B A M L   `   `   `  ����P  LProgrammierprojektWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:ProgrammierprojektWPFProgrammierprojektWPF  5   	   $    ProgrammierprojektWPF.IPWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:ProgrammierprojektWPF     5   	     � Title$  Enter IP and Port��5   	   $	��100��6#   $	��500��60   $��0q�6<   ���	#FF2B2B2B$��Bell MT)�6g   $��16(�6|   
���   � � 
ResizeMode$ NoResize=�6�    � WindowStartupLocation$ CenterScreen=�6�    � 	Resources 5
       �� 
TargetType �� Property �� Value �� 
TargetType �� RadiusX	 �� RadiusY
 �� Triggers P� Property P� Value��%�  '��       '�� p    �� 5   
   " ��6(   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      �� #��z��6(   #��z��6L   $ 5��6r   $	 5��6~   �� 5      $��Center=�6/   $��Center=�6L   5      5      5      �� 5       � �� �	#FFC8C8C86+   
 5      P� 5       �@J�IsMouseOver . . �5      ��  � �� �	#FF98989863   5      P� 5       � �� . . �5      ��  � �� �	#FF68686863   5      P� 5       � {� .  . �5      ��  � �� �	#7FC8C8C863   �� 5        � �� �	#7F00000063   5!      5#      �� 5%   
   " ��6)   .�5&      ��  � �� �	#FFC8C8C86+   5'      5(      .��5*      �  � ColumnDefinitions 5+   
   �� 5,      $
��300* �6   �� 5-      $
��150* �6   .+�5/   
   �� -   5         ��Name$ 
cmdConfirm��5/      $��Confirm��6%   $��5,0,5,0q�67   $��20��6H   $��1��6T   6d   �� 51   
   -   5         ��Name$
 tbIP��51      $��23��6    $��5,10,5,0q�6,    �� TextWrapping$ NoWrap=�6>   $	��Top=�6T    � ToolTip$ Enter the IP here.��6l   �� 52   
   -   5        $ tbPort��52      $��23��6"   $��5,38,5,0q�6.   $ NoWrap=�6@   $	��Top=�6V   $ Enter the port here.��6n   53      54      