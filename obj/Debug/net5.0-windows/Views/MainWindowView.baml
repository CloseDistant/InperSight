   M S B A M L   `   `   `  ????E  AInperSight, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:InperSight.ViewsInperSight.Views  5   	   =#clr-namespace:InperSight.ViewModelsInperSight.ViewModels  5	   	   %    InperSight.Views.MainWindowView   5      W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:InperSight.Views     5   	   S OHandyControl, Version=3.3.0.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5   	   A =Stylet, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null)s!https://github.com/canton7/Stylet  5   	   ,vm#clr-namespace:InperSight.ViewModels  5	   	   -   5        &   HandyControl.Controls.BlurWindow   Name$  
MainWindow??5   g      
InperTheme#???   5   	     DarkMaskBrush"  HandyControl.Controls.Window!   CloseButtonHoverBackground# ?  5   	   !   CloseButtonHoverForeground
 ?????   OtherButtonForeground
 ?????!   OtherButtonHoverForeground
 ?????   CloseButtonForeground
 ?????   NonClientAreaBackground# ?   5   	      NonClientAreaForeground
 ????? ? Icon$ /InperSight.ico??5   	   	 ? Title$	 inper??5   	   
   	ShowTitle$
 False??6   "  HandyControl.Controls.Dialog   Token$ 
MainDialog??6*   $
??1000??6G   $	??660??6W   .??5      ?   InperMainPageColor#??[ 6    ? RowDefinitions 5   
   ?? 5      $??66 ?6   ?? 5      $??* ?6   .+?5   
   7  1HandyControl.Controls.TransitioningContentControl  $??0??6)      TransitionMode$ 
Top2Bottom=?66   .??5      ? $??0 5 0 0q?6     InperTitlePageColor#??[ 6$    ? ColumnDefinitions 5      ?? 5      ?? 5      $
??.55* ?6'   .+?5!      ?? -   5         ??Name$ manulControl??5!   1   $??0??6!     Stylet.Xaml.View   Model 6G   ?? *ManulControlViewModel+d? 5"      -   5        $??buttonVisibilitySwitch??5"   ?   $??1??6   $y?	Collapsed=?6/     ToggleButtonSwitch#??[ 6F    d? 	IsChecked$
 TrueY?6?    d? Checked 6r   !  Stylet.Xaml.ActionExtension  * ButtonVisibilitySwitch_Checked+ d? 	Unchecked 6?     *" ButtonVisibilitySwitch_Unchecked+6?   5#      5$      ?? 5%   
   $??Bottom=?6   ???#eee$??1q?6?   ? 5&   
   -   5        $
??main??5&      $??1??6   .+?5+        $ 
Left2Right=?6-   .??5,      ??  6!   ?? *
ActiveItem+5-      52      ?? 53   
   -   5         ??Name$ 
ResizeGrip??53   ,   $??10??6   $??10??6    $??Right=?6@   $??Bottom=?6\   $??1??6w   $??False??6?   $y?Visible=?6?   '  !System.Windows.Shell.WindowChrome   ResizeGripDirection$ BottomRight=?6?   ?? 54   
   $??2??6   $??Hidden=?6(   $??Right=?6M    K? ZIndex$
 9999??6i   .??55      ?? !  HandyControl.Controls.Growl   Token$ 
SuccessMsg??6   $	??Top=?66   $??
0,10,10,10q?6N   56      57      58      