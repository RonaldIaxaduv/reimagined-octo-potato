   M S B A M L   `   `   `  ����P  LProgrammierprojektWPF, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:ProgrammierprojektWPFProgrammierprojektWPF  5   	   '    !ProgrammierprojektWPF.LoginWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:ProgrammierprojektWPF     5   	     � Title$  Login / Register��5   	   $	��100��6"   $	��500��6/   $��0q�6;   ���	#FF2B2B2B$��Bell MT)�6f   $��16(�6{   
���   � � 
ResizeMode$ NoResize=�6�    � WindowStartupLocation$ CenterScreen=�6�    � 	Resources 5
       �� 
TargetType �� Property �� Value �� 
TargetType �� RadiusX	 �� RadiusY
 �� Triggers P� Property P� Value��%.  '��       '�� p    'G� �    �� 5   
   " ��6(   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      � .+�5      �� #��z��6(   #��z��6L   $ 5��6r   $	 5��6~   �� 5      $��Center=�6/   $��Center=�6L   5      5      5      �� 5       � �� �	#FFC8C8C86+   
 5      P� 5       �@J�IsMouseOver . . �5      ��  � �� �	#FF98989863   5      P� 5       � �� . . �5      ��  � �� �	#FF68686863   5      P� 5       � {� .  . �5      ��  � �� �	#7FC8C8C863   �� 5        � �� �	#7F00000063   5!      5#      �� 5%   
   " ��6)   .�5&      ��  � �� �	#FFC8C8C86+   5'      �� 5(   
   " G�6-   .�5)      ��  � �� �	#FFC8C8C86+   5*      5+      .��5-      �  � ColumnDefinitions 5.   
   �� 5/      $
��300* �6   �� 50      $
��150* �6   .+�52   
   �� -   5         ��Name$ cmdLogin��52      $��Login��6#   $��5,11,5,0q�63   $	��Top=�6E   $��22��6]   $��1��6i   6y   �� 53   
   -   5        $ cmdRegister��53      $��Register��6&   $��5,36,5,0q�69   $	��Top=�6K   $��22��6c   $��1��6o   6   �� 55   
   -   5         ��Name$ 
tbUsername��55      $��23��6&   $��5,10,5,0q�62    �� TextWrapping$ NoWrap=�6D   $	��Top=�6Z    � ToolTip$ Enter your username here.��6r   G� 56   
   -   5         G�Name$ 
pbPassword��56      $��5,38,5,0q�6*   $	��Top=�6<   $��23��6T   $ Enter your password here.��6`   $��2q�6�   57      58      