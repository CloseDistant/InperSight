   M S B A M L   `   `   `  ????P  LInperStudioControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullzQclr-namespace:InperStudioControlLib.Control.Window;assembly=InperStudioControlLib$InperStudioControlLib.Control.Window  5   	   E AInperSight, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:InperSight.ViewModelsInperSight.ViewModels 5   	   9!clr-namespace:InperSight.Lib.BeanInperSight.Lib.Bean 5
   	   3clr-namespace:InperSight.ViewsInperSight.Views 5   	   )   #InperSight.Views.DataPathConfigView   5      W SPresentationCore, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=7.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   cinperQclr-namespace:InperStudioControlLib.Control.Window;assembly=InperStudioControlLib     5   	   2vm#clr-namespace:InperSight.ViewModels     5   	   A =Stylet, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null)s!https://github.com/canton7/Stylet  5   	   S OHandyControl, Version=3.3.0.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5	   	   .global!clr-namespace:InperSight.Lib.Bean  5
   	   *localclr-namespace:InperSight.Views  5   	   <   6InperStudioControlLib.Control.Window.InperDialogWindow    IsShowTopAllButton$  False??5   	      IsShowOtherButton$ False??6$      InperFontFamily#???   6>   $	??360??5   	   $	??280??6   $	??200??6"   $	??200??61    ? Title$ Config??6A      IsShowCancleButton$ False??6P      ConfirmClickEvent 6k   !  Stylet.Xaml.ActionExtension  *%#InperDialogWindow_ConfirmClickEvent+ ? 	Resources 5      *  $InperSight.Lib.Bean.InperGlobalClass  InperGlobalClass??%   &	         5   
   6"   5      .??5      ?   InperDialogContentColor#???  6   .+?5   
   ?? -   5         ??Name$
 Path??5      $y?Visible=?6$   $??	30 20 0 0q?69   .+?5      ? .+?5      ?? #??[  6   $??35??6J   $??11(?6U   $
??Name??6c   $??Center=?6o   ?? 5      -   5         ??Name$ fileName??5   ?  $??20 0 0 0q?6     	BaseStyle#??[ 6-   $??Center=?6Q   $??1q?6s   ???#CCCCCC)  #HandyControl.Controls.BorderElement   CornerRadius$ 0??6?   $??11(?6?   $	??180??6?   $??20??6?   '  !HandyControl.Controls.InfoElement	   Placeholder$	 	File name??6e  ??6?   ?? 
 ?? Source#
 [  ?? Path$ DataFolderName? ?? Mode$ OneWay=? ?? UpdateSourceTrigger$ PropertyChanged=?6?  5      ? 5      $
??0 15q?6   .+?5      ?? #??[  6   $??11(?6J   $??35??6X   $??Where??6c   $??Center=?6p   ?? 5      -   5         ??Name$ pathText??5      $??20 0 0 0q?6+   $??1q?6=   $??11(?6Q   ???#CCCCCC$	??180??6?     InperTextColor#??[ 6?   #??[ 6  $ 0??6A  $??20??6c  #???   6o  $
??Path??6?   ? ToolTip 6_   ?? #
 [ $ DataPath?$ PropertyChanged=???6?    *
ChoosePath+??6?  ?? #
 [ $ DataPath?$ TwoWay=?$ PropertyChanged=?5      ? 5      $??Stretch=?6   .+?5      ?? #??[  6   $??11(?6J   $??35??6X   $	??Tag??6c   $	??Top=?6n   ?? 5      $??20 0 0 0q?6   $??Stretch=?6/   .+?5      (  "HandyControl.Controls.TagContainer   ?? DisplayMemberPath$ Value??6&   $??0 0 0 3q?6A   $	??180??6R     InperSmallFontSize#??[ 6^   $??5q?6?   $??58??6?   $
??Left=?6?   ??6?   ?? *
DataList+]? 5       $
??True??6    $??0q?65   $
??Left=?6A   $	??180??6\   .+?5!      ?? #??[  6A   $??10,0,0,0q?6o   $??Right=?6?   $??Add Tag??6?   ??6!     *
AddItemCmd+6?   ?? 5"      #??[  6"   #??[ 6P   ??6   ?? *	TagName+$ TwoWay=?5#      5$      5%      5&      ? 5'   
   -   5         ?Name$
 Load??5'      $y?	Collapsed=?6   .+?5(      ? $??Center=?6   $??Center=?63   $??30??6P   .+?5)      ??   ButtonPrimary#??[ 6   #???   6@     
InperTheme#???  6?   $
??Path??6?   $
??Load??6?   $ 4 0 0 4??6?   ??6o     *
ChoosePath+6?   ?? 5*      -   5        $ loadPath??5*   ?   $	 Storage Path??6     TextBoxExtend#???  6D   $	??200??6l   ${?False??6{   $ 0??6?   $
??10 0q?6?   6?   5+      5,      ? 5-   
   -   5        $
 Save??5-      $y?	Collapsed=?6   .+?5.      ? .+?5/      ??  	 InperFreeButton#??? 	 6   6E   50      51      52      53      