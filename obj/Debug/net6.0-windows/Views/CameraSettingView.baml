   M S B A M L   `   `   `  ����P  LInperStudioControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullzQclr-namespace:InperStudioControlLib.Control.Window;assembly=InperStudioControlLib$InperStudioControlLib.Control.Window  5   	   E AInperSight, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:InperSight.Lib.HelperInperSight.Lib.Helper 5   	   3clr-namespace:InperSight.ViewsInperSight.Views 5   	   9!clr-namespace:InperSight.Lib.BeanInperSight.Lib.Bean 5	   	   =#clr-namespace:InperSight.ViewModelsInperSight.ViewModels 5   	   (   "InperSight.Views.CameraSettingView   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ainpQclr-namespace:InperStudioControlLib.Control.Window;assembly=InperStudioControlLib     5   	   6global#clr-namespace:InperSight.Lib.Helper     5   	   0localclr-namespace:InperSight.Views     5   	   2bean!clr-namespace:InperSight.Lib.Bean     5	   	   S OHandyControl, Version=3.3.0.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5
   	   A =Stylet, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null)s!https://github.com/canton7/Stylet  5   	   ,vm#clr-namespace:InperSight.ViewModels  5   	   -   5        $��camera��5   o   <   6InperStudioControlLib.Control.Window.InperDialogWindow    IsShowTopAllButton$  False��5       � 
ResizeMode$ NoResize=�65    � Title$ Insight��5   	   $	��700��6   $	��595��6%      IsShowCancleButton$ False��6}      IsShowOtherButton$ False��6�    � 	Resources 5      *  $InperSight.Lib.Bean.InperGlobalClass   InperGlobalClass-  'InperSight.Lib.Helper.InperDeviceHelper  InperDeviceHelper  bgColor  btnTextColor1  btnTextColor2��%�   &	        &	      &	 &     &	 E     &	 d       5   
   6      5   
   6#   ��5   
   #F2F2F2��63   ��5   
   #FFFFFF��69   ��5   
   #666666��69   5      .��5      � .+�5!   
   � #��[ 6    � ColumnDefinitions 5"      �� 5#      $
��1.5* �6#   �� 5$      $��1 �6#   �� 5%      $��* �6#   .+�5(      � $��0��6   $�� 0 0 0 0q�6#    � RowDefinitions 5)      �� 5*      $��* �6$   �� 5+      $
��.25* �6$   .+�5.      �� $��0��6   $��
25 20 25 0q�6&     InperBorderColor#��[ 6:   $��1q�6j   ._�5/      � .+�51      �� -   5        $��drawAreaCanvas��51   9   $	��Top=�6!   
������  K� ZIndex$	 999��6*  $	��351��6=  $	��351��6J  	 J� MouseLeftButtonDown	 6O   !  Stylet.Xaml.ActionExtension  *Image_MouseLeftButtonDown+
 J� MouseLeftButtonUp
 6�     *Image_MouseLeftButtonUp+ J� 	MouseMove 6�     *Image_MouseMove+ J� 
MouseLeave 6�     *Image_MouseLeave+6J  �� 53      $��Uniform=�6    $	��Top=�62   ��6J   �� *InperDeviceHelper.WBMPPreview+ �� UpdateSourceTrigger$ PropertyChanged=�� 55      $��Bottom=�6$   $��0 0 0 0q�6?   $��32��6P   ���#E8E8E8.+�56      �� -   5         ��Name$
 home��56   w     InperFreeButton#��[ 6%   $��50��6N   $��20��6Y   $��10 8 0 0q�6e    � Cursor$
 Hand��6�   ��6�     *HomeShowEvent+.��57   "   �� -   5         ��Name$	 bd1��57   c     
InperTheme#��[ 6)    �� CornerRadius$ 2��6R   ._�58   &   �� -   5        $��t1��58   <   $
��Home��60   $��Center=�6F   $��Center=�6c   
�������$��14(�6�   59   #    � Triggers 5:   "   :� 5;   &    :� RoutedEvent$ Button.Click��63   .[�5<   *   �� .��5=   .   �� .��5>   2   T�  g� 	BeginTime	 0:0:06P    �� 
TargetName$	 bd1��6b    �� TargetProperty$ 
Background�6~   ..�5?   6   i�  R� KeyTime$ 0:0:0.1��6N    R� Value# [ 6`   5@   3   T� 5A   2   	 0:0:06P   $	 bd2��6b   $ 
Background�6~   ..�5B   6   i� $ 0:0:0.1��6N   # [ 6`   5C   3   T� 5D   2   	 0:0:06P   $ t1��6b   $ 
Foreground�6}   ..�5E   6   i� $ 0:0:0.1��6N   # [ 6`   5F   3   T� 5G   2   	 0:0:06P   $ t2��6b   $ 
Foreground�6}   ..�5H   6   i� $ 0:0:0.1��6N   # [ 6`   5I   3   5J   /   5K   +   5L   '   5N      �� 5O      -   5        $ frame��5O   r   #��[ 6%   $��50��6�   $��20��6�   $��10 8 0 0q�6�   ��6N     *FrameShowEvent+.��5P   "   �� -   5        $	 bd2��5P   :   $ 2��6)   ._�5Q   &   �� -   5        $��t2��5Q   =   $��Frame��60   $��Center=�6G   $��Center=�6d   ���#666666$��14(�6�   5R   #    5S   "   :� 5T   &   $ Button.Click��63   .[�5U   *   �� .��5V   .   �� .��5W   2   T� 	 0:0:06P   $	 bd2��6b   $ 
Background�6~   ..�5X   6   i� $ 0:0:0.1��6N   # [ 6`   5Y   3   T� 5Z   2   	 0:0:06P   $	 bd1��6b   $ 
Background�6~   ..�5[   6   i� $ 0:0:0.1��6N   # [ 6`   5\   3   T� 5]   2   	 0:0:06P   $ t2��6b   $ 
Foreground�6}   ..�5^   6   i� $ 0:0:0.1��6N   # [ 6`   5_   3   T� 5`   2   	 0:0:06P   $ t1��6b   $ 
Foreground�6}   ..�5a   6   i� $ 0:0:0.1��6N   # [ 6`   5b   3   5c   /   5d   +   5e   '   5g      �� 5h      #��[ 6%   $��30��6N   $��20��6Y   $��	130 8 0 0q�6e   $
 Hand��6�   ��6x     *DeltaFModeEvent+.��5i   "   �� -	   5         ��Name$	 tb3��5i   ,   $��Center=�69   $
��DF/F��6T   $��Center=�6`   #��[ 6}   5j      �� 5k      #��[ 6%   $��20��6N   $��20��6Y   $
 Hand��6e   $��	10 8 10 0q�6s   ��6�     *CutViewEvent+.��5l   "   �� $ ��/Lib/Images/Camera/cut.png��6(   $��UniformToFill=�6L   5m      �� 5n      #��[ 6%   $��20��6N   $��20��6Y   $
 Hand��6e   $��0 8 0 0q�6s   ��6�     *ShowImagePath+.��5o   "   �� $!��/Lib/Images/Camera/path.png��6(   $��UniformToFill=�6M   5p      5q      5r      5s      � 5u      $��1��6    5v      �� 5w      $��* �6(   �� 5x      $��* �6(   .+�5z      � $��0��6   .+�5{      � $��Right=�6$   $��Center=�6@   $��0 5 25 0q�6[   .+�5|      �� $��Frame Setting��6(   $��0 0 5 0q�6=   �� 5}      ${�False��6(   $��30��6:   $��0 0 0 0q�6E   #��[ 6V    �� TextAlignment$ Center=�6�   $��Center=�6�   $��12(�6�   ��6   ��  �� ElementName$ zoomCoefficient�� �� Path$ Value� �� StringFormat$ {0:f2}�� � ToolTip 5~   "   �� 5   &   ��60   �� $ zoomCoefficient��$ Value�5�      �� 5�      -
   5        $��zoomCoefficient��5�   �   
�������$��90��6�   $��5 0 10 0q�6�    � Maximum$ 16Q�6    � Minimum$  2Q�6  ! � Value! 6%   �� *CameraSetting.ZoomCoefficient+" �� Mode$" OneWay=�$ PropertyChanged=�# � ValueChanged# 6�     *ZoomCoefficient_ValueChanged+6  �� 5�      #��[ 6%   $��20��6N   $��20��6Y   $	��5 0q�6e   $
 Hand��6r   ��6�     *FrameAddEvent+.��5�   "   �� $ ��/Lib/Images/Camera/add.png��6(   $��UniformToFill=�6L   5�      �� 5�      #��[ 6%   $��20��6N   $��20��6Y   $	��5 0q�6e   $
 Hand��6r   ��6�     *FrameDeleteEvent+.��5�   "   �� $#��/Lib/Images/Camera/reduce.png��6(   $��UniformToFill=�6O   5�      5�      5�      � 5�      $��1��6    $
��25 0q�6-   $��Right=�6;   .+�5�      �� $
��Name��6$   $��Center=�61   �� 5�      -   5        $ ��Name$$ 
neuronName��5�   m   $��Center=�6"   $
��10 0q�6=   $��Neuron 1��6K   ${�False��6[   $��80��6�   $��20��6�   �� 5�      $��Neuron��6$   $��Center=�62   �� 5�      -   5        % ��Name$% roiType��5�   #   $��Center=�64   $
��10 0q�6O   $��85��6]    6h   �� $ roiType��$ SelectedValue.Content�.@�5�      �� & �� 
IsSelected$
& True��6+   .��6=   Circle6E   �� 5�      .��6+   	Rectangle66   �� 5�      .��6+   	Polygon64   5�      �� 5�      -   5        $��	addRoiBut��5�   k   #��[ 6!   $��20��6|   $��20��6�   $	��5 0q�6�   $
 Hand��6�   ��6J     *AddRoiEvent+.��5�      �� $ ��/Lib/Images/Camera/add.png��6$   $��UniformToFill=�6H   5�      �� 5�      -   5        $��addInperThemeRoiBut��5�   J   #��[ 6!   $��20��6e   $��20��6p   $	��5 0q�6|   $
 Hand��6�   $y�	Collapsed=�6�   .��5�      �� $*��$/Lib/Images/Camera/addInperTheme.png��6$   $��UniformToFill=�6R   5�      �� 5�      -   5        $��deleteRoiBut��5�   n   #��[ 6!   $��20��6�   $��20��6�   $	��5 0q�6�   $
 Hand��6�   ��6J     *DeleteRoiEvent+.��5�      �� $#��/Lib/Images/Camera/reduce.png��6$   $��UniformToFill=�6K   5�      5�      5�      5�      �� 5�      $��1��6   $��1��6%   #��[ 6/   �� 5�      $��2��6   $��	10 20 0 0q�6)   {�6<   �� ' �� Source#' [  $ IsRecord�  Boolean2BooleanReConverter( �� 	Converter#( [ $ PropertyChanged=�.+�5�      � .+�5�      �� $��Settings��6    #��[ 60   $��16(�6Y   �� 5�      $	��150��6   $��10 0 0 0q�6)   $��1��6;   $��Center=�6F   #��[ 6a   ) J� Opacity$) .6Q�6�   5�      � 5�      $��10q�6   .+�5�      �� $	��FPS��6    ���#333333$��Center=�6@   � 5�      $��Right=�6    .+�5�      �� -   5        $	% fps��5�   #   $	��124��6�   $��20��6�   * �� SelectionChanged* 60     *FpsAndGain_SelectionChanged+  Stylet.Xaml.View+   ActionTarget+ 6j   �� .@�5�      �� .��6+   106/   �� 5�      .��6+   156/   �� 5�      .��6+   206/   �� 5�      .��6+   256/   �� 5�      .��6+   306/   5�      5�      5�      � 5�      $��10q�6   .+�5�      �� $
��Gain��6    ���#333333$��Center=�6A   � 5�      $��Right=�6    .+�5�      �� -   5        $
% gain��5�   #   $	��124��6�   $��20��6�   * 61     *FpsAndGain_SelectionChanged++ 6k   �� .@�5�      �� .��6+   Low60   �� 5�      .��6+   Medium63   �� 5�      .��6+   High61   5�      5�      5�      � 5�      $��10q�6   .+�5�      �� $��Focus Plane��6    ���#333333$��Center=�6H   � 5�      $��Right=�6    .+�5�      �� ${�False��6$   $��30��66   $��0 0 3 0q�6A   #��[ 6R   $ Center=�6�   $��Center=�6�   $��12(�6�   ��6{   �� $ 
focusPlane��$ Value� 5�      �� 5�   "   ��6,   �� $ 
focusPlane��$ Value�5�      �� 5�      #��[ 6!   $��20��6J   $��20��6U   $
 Hand��6a   $��
focusPlane��6�   ��6o     *Param_Reduce_Event+.��5�      �� $#��/Lib/Images/Camera/reduce.png��6$   $��UniformToFill=�6K   5�      �� 5�      -   5        , ��Name$, 
focusPlane��5�   �   
�������- �� IsSnapToTickEnabled$
- True��6�   $��84��6�   $
  -127Q�6�   $	 127Q�6  ! 6!   �� *CameraSetting.FocusPlane+$" OneWay=�$ PropertyChanged=�# 6�     *Slider_ValueChanged+6  �� 5�      #��[ 6!   $��20��6J   $��20��6U   $
 Hand��6a   $��
focusPlane��6�   ��6o     *Param_Add_Event+.��5�      �� $ ��/Lib/Images/Camera/add.png��6$   $��UniformToFill=�6H   5�      5�      5�      � 5�      $��10q�6   .+�5�      �� $��Alpha ��6    ���#333333$��Center=�6C   � 5�      $��Right=�6    .+�5�      �� ${�False��6$   $��30��66   $��0 0 3 0q�6A   #��[ 6R   $ Center=�6�   $��Center=�6�   $��12(�6�   ��6{   �� $ 
upperLevel��$ Value�$ {0:f2}�� 5�      �� 5�   "   ��6,   �� $ 
upperLevel��$ Value�5�      �� 5�      #��[ 6!   $��20��6J   $��20��6U   $
 Hand��6a   $��
upperLevel��6�   ��6p     *Param_Reduce_Event+.��5�      �� $#��/Lib/Images/Camera/reduce.png��6$   $��UniformToFill=�6K   5�      �� 5�      -   5        $, 
upperLevel��5�   �   
�������$��84��6�   $  0Q�6�   $ 10Q�6�   ! 6!   �� *CameraSetting.UpperLevel+$" OneWay=�$ PropertyChanged=�# 6�     *Slider_ValueChanged+6�   �� 5�      #��[ 6!   $��20��6J   $��20��6U   $
 Hand��6a   $��
upperLevel��6�   ��6o     *Param_Add_Event+.��5�      �� $ ��/Lib/Images/Camera/add.png��6$   $��UniformToFill=�6H   5�      5�      5�      � 5�      $��10q�6   .+�5�      �� $��Beta ��6    ���#333333$��Center=�6B   � 5�      $��Right=�6    .+�5�      �� ${�False��6$   $��30��66   $��0 0 3 0q�6A   #��[ 6R   $ Center=�6�   $��Center=�6�   $��12(�6�   ��6{   �� $ 
lowerLevel��$ Value�$ {0:f2}�� 5�      �� 5�   "   ��6,   �� $ 
lowerLevel��$ Value�5�      �� 5�      #��[ 6!   $��20��6J   $��20��6U   $
 Hand��6a   $��
lowerLevel��6�   ��6p     *Param_Reduce_Event+.��5�      �� $#��/Lib/Images/Camera/reduce.png��6$   $��UniformToFill=�6K   5�      �� 5�      -   5        $, 
lowerLevel��5�   �   
�������$��84��6�   $  0Q�6�   $	 100Q�6�   ! 6!   �� *CameraSetting.LowerLevel+$" OneWay=�$ PropertyChanged=�# 6�     *Slider_ValueChanged+6�   �� 5�      #��[ 6!   $��20��6J   $��20��6U   $
 Hand��6a   $��
lowerLevel��6�   ��6o     *Param_Add_Event+.��5�      �� $ ��/Lib/Images/Camera/add.png��6$   $��UniformToFill=�6H   5�      5�      5�      � 5�      $
��0 20q�6   .+�5�      �� $��Excitation Light��6    #��[ 68   $��16(�6a   �� 5�      $	��100��6   $��10 0 0 0q�6)   $��1��6;   $��Center=�6F   #��[ 6a   $) .6Q�6�   5�      � 5�      $��10q�6   .+�5�      �� $��	470 nm(%)��6    ���#333333$��Center=�6F   � 5�      $��Right=�6    .+�5�      �� ${�False��6$   $��30��66   $��0 0 3 0q�6A   #��[ 6R   $ Center=�6�   $��Center=�6�   $��12(�6�   ��6{   �� $ excitLowerLevel��$ Value� 5�      �� 5   "   ��6,   �� $ excitLowerLevel��$ Value�5     �� 5     #��[ 6!   $��20��6J   $��20��6U   $
 Hand��6a   $��excitLowerLevel��6�   ��6o     *Param_Reduce_Event+.��5     �� $#��/Lib/Images/Camera/reduce.png��6$   $��UniformToFill=�6K   5     �� 5     -   5        $, excitLowerLevel��5  �   
�������$
- True��6�   $��84��6�   $  0Q�6  $	 100Q�6  ! 6!   �� *CameraSetting.ExcitLowerLevel+$" OneWay=�$ PropertyChanged=�# 6�     *Slider_ValueChanged+6  �� 5     #��[ 6!   $��20��6J   $��20��6U   $
 Hand��6a   $��excitLowerLevel��6�   ��6o     *Param_Add_Event+.��5     �� $ ��/Lib/Images/Camera/add.png��6$   $��UniformToFill=�6H   5	     5
     5     5     5     5     5     