   M S B A M L   `   `   `  ????E  AInperSight, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:InperSight.ViewsInperSight.Views  5      9!clr-namespace:InperSight.Lib.BeanInperSight.Lib.Bean  5      P LInperStudioControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null|Rclr-namespace:InperStudioControlLib.Control.TextBox;assembly=InperStudioControlLib%InperStudioControlLib.Control.TextBox 5	      ?$clr-namespace:InperSight.Lib.ConvertInperSight.Lib.Convert  5      =#clr-namespace:InperSight.ViewModelsInperSight.ViewModels  5      ;"clr-namespace:InperSight.Lib.ChartInperSight.Lib.Chart  5      *    $InperSight.Views.DataShowControlView   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      A =Stylet, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null)s!https://github.com/canton7/Stylet  5      *localclr-namespace:InperSight.Views  5      .global!clr-namespace:InperSight.Lib.Bean  5      ^inperRclr-namespace:InperStudioControlLib.Control.TextBox;assembly=InperStudioControlLib  5	      \ XSciChart.Charting, Version=6.6.0.26505, Culture=neutral, PublicKeyToken=b55dd9efe817e823X	 TSciChart.Core, Version=6.6.0.26505, Culture=neutral, PublicKeyToken=b55dd9efe817e823X
 TSciChart.Data, Version=6.6.0.26505, Culture=neutral, PublicKeyToken=b55dd9efe817e823[ WSciChart.Drawing, Version=6.6.0.26505, Culture=neutral, PublicKeyToken=b55dd9efe817e8238sc)http://schemas.abtsoftware.co.uk/scichart  	 
  5
      8convert$clr-namespace:InperSight.Lib.Convert  	 
  5      2vm#clr-namespace:InperSight.ViewModels  	 
  5      S OHandyControl, Version=3.3.0.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5      .chart"clr-namespace:InperSight.Lib.Chart  5        ? 	Resources  5      *   $InperSight.Lib.Bean.InperGlobalClass   InperGlobalClass6   0InperSight.Lib.Convert.ChannelNameSplitConverter  ChannelNameSplitConverter  LeftAxisLabelStyle ?? 
TargetTypeD  >SciChart.Charting.Visuals.Axes.LabelProviders.DefaultTickLabel ?? Property ?? Value ?? TextAlignment1  +SciChart.Charting.Visuals.SciChartScrollbar ?? 
TargetType ?? CornerRadius  
InperTheme J? Opacity  ListDataTemplate ?Name	 ?? UpdateSourceTrigger
 ? ColumnDefinitions ??Name ?? 	Converter ? LayoutTransform ?? Angle ? ToolTip  InperFreeButton!  Stylet.Xaml.ActionExtension ?? RelativeSource ?? Mode ?? Stretch ?? Click  InperReduce/  )SciChart.Charting.Visuals.SciChartSurface  Name8  2SciChart.Charting.Visuals.TradeChart.SciChartGroup   VerticalChartGroup   XAxis   RenderableSeries=  7SciChart.Charting.Common.MarkupExtensions.SeriesBinding   UpdateSourceTrigger   ViewportManager   YAxes0	  *SciChart.Charting.Visuals.Axes.NumericAxis-
  'SciChart.Charting.Visuals.Axes.AxisCore 
  TickLabelStyle 
  DrawMajorBands 
  GrowByK  ESciChart.Charting.Common.Databinding.StringToDoubleRangeTypeConverter 
  DrawMajorGridLines 
  DrawMinorGridLines  
  MaxAutoTicks! 
  TextFormatting-  'SciChart.Charting.Visuals.Axes.AxisBase"   AxisAlignment# 
  	AutoRange$ 
  VisibleRange% ?? Mode??%o  &	        &	      &	 &     '      7  &	 a    7  7  7  7  7  7    5   
   6"     5   
   6,   ?? 5   
   "  6+   .?5      ??  ? ??$ 0q?6'   ?? 5       ? ?? 5      ?? 5      .R?5      ?? .+?5      ?? $??50??6(   $ Right=?63   ??6I   ?? *Text+5      5      5      5      ?? 5   
   "  6   .?5       ??  ?@ GripsThickness$ 10Q?6/   ?? 5!       ? ??
 ???? 6+   ?? 5#       ?@ 
GripsStyle 5$      ?? 5%      " ??6   .?5&      ??  ? ??$ Bottom=?6>   ?? 5'       ? ?? 5(      ?? 5)   "   " ??62   .R?5*   &   ?? $??1q?6-   $ 3??6A   8??  $ .5Q?6|   ???#F1F1F1$??20??6?   5,   '   5-   #   5/      50      52      ?? 54       ?@ ViewportStyle 55      ?? 56      " ??6   .?57      ??  ? ?? 58      ?? 59   "   " ??62   .R?5:   &   ?? $??0q?6-   
???   ?
?????w?$ .8Q?6q   5;   #   5=      5>      5@      ?? 5B       ?@ NonSelectedAreaStyle 5C      ?? 5D      " F?6   .?5E      ??  ? ??
 ???? 61   ?? 5F       ? {?$ False??66   5G      5I      5J      ?? 5K   
   .R?5L      ? $ 	chartItem??6   
?????? $
??0 -1q?6?   ??6M   ?? *Height+$	 PropertyChanged=?
 5M      ?? 5N      $??40 ?6'   ?? 5O      $??40 ?6'   ?? 5P      $??* ?6'   .+?5R      ?? $??3??6   
???????$ 1Q?6A   $??0 0 0 1q?6M   ?? 5S      $??0??6   
???????$??0 0 1 0q?6=   ._?5T      ?? $
 test??6/   $??30??6    $??CharacterEllipsis=?6>   $	??2 5q?6_   $ Center=?6l   $??Center=?6?   $??Center=?6  $	??100??6+  ??6?   ?? *Name+$	 PropertyChanged=?8    5U      ?? 5V      $ 90Q?6.    5X      ?? 5Y      ??6(   ?? *Name+5[      5\      ? 5]      $??1??6   .+?5^      ?? 
???????$??0 0 1 0q?61   ?? 5_      $??Center=?6!   $??Center=?6<   .+?5`      ?? $??0 0 0 0q?6?   8?? $??20??6?   $??20??6?   ??6!     *
YaxisAdd+??6?   ?? *DataContext+ ?? $
 Self=?.??5a      F? ???#1875F0$ Uniform=?62   ?????0????????????????NZ?????%`?k????$7????`????????%????NZ???????ѐNZ?????%??????`?????$7Ӏ??`?k?%????NZ??????????????0??????xH`?@Ys?$7@Ys?`?k@Ys????xH???0?????????0????ӰxH???@Ys`?k@Ys?$7?@Ys`??xH???0??????$7    ???U    ??U?$7?    ?r??U?In
?$7?In
??r?In
?In
?r?In
?$7S?In
أp=
?C@?r?$7?PQ`PQ`??G?z?`@`s??|0 	@e???U	?]???(?Z@00?	 ?????	??6???	?`????	?z?G?P@P~?	@Hy??U	Sp???(\?c@P?v`s?0\PQ`S??G?z?`@P?Q?|?&@e?SP}? ]???(?Z@??  ???? ??6??? `?0/? ?z?G?P@?&@Hy?P?Qp? :?P?v0\0\?@~v?p?P?Q@Hy?&? FP}? `??? ??6?? ? ???? ?gg0/? @e??&?أp=
g_@P?Q0? :?PQ`0\?p??@~v0 	p???U	@Hy?00?	 F???	`????	??6Ӡ??	 ??P~?	?gg??U	@e??0 	 ?|`s?0?PQ`PQ`	6D   5c      ?? 5d      $??0 15 0 0q?6A   8?? $??20??6|   $??20??6?   $??Center=?6?    6"     *YaxisNormal+.??5f      F? ???#1875F0$ Uniform=?62   ?????    ??v?@x}??v?@x}0>??p?{0>??p?{?hx?    ?hx?    ??v?@x}@x}?    @x}?    p?{?p?{p?{?p?{    ?@x}    ?@x}    @x}@x}@x}@x}??hx0>????v0>????v??v?0>???v?0>??hx??hx?hx??hx0>????v@x}???v    ??hx    ??hxp?{?0>?p?{?0>?@x}???v@x}	6D   5h      ?? 5i      $
??0 15q?6?   8?? $??20??6?   $??20??6?   ??6!     *YaxisReduce+??6B   ?? *DataContext+ ?? $
 Self=?.??5j      F? ???#1875F0$ Uniform=?62   8?? 6D   5l      5m      5n      ? 5o      $??2??6   .+?5p        $ sciChartSurface??6M  
?????? $ SyncedChartsSameAxisSizes??6B    6?   ?? *TimeSpanAxis+ 6?     *RenderableSeries+$ PropertyChanged=? 6?   ?? *ViewportManager+$	 PropertyChanged=? 5r      	  8  $ False??6b   $ 0.1,0.1 6y   $??0??6?   $??0??6?   $??60??6?   $ False??5s   !   $ False??5t   !   $  6??6<   $! #0.000??6M   $??Right=?6e   $??0 0 1 0q?6?   
???????$
" Left=?6?   $# Always=?6?   $ 6?   ?? *YVisibleRange+$% TwoWay=?$	 PropertyChanged=?5v      5w      5x      5y      5z      .??5{      ? & ? RowDefinitions& 5|   
   ?? 5}      $??* ?6   ?? 5~      $
??auto ?6   ?? 5      $
??auto ?6   .+?5?   
   ? $??0??6   .+?5?      ?? -   5        $??
dataScroll??5?      .??5?      ? & 5?      ?? 5?      $??* ?6(   ?? 5?      $
??auto ?6(   .+?5?      ?? -   5        ' ??Name$' dataList??5?      $??	-3 0 0 -3q?60   .  (HandyControl.Controls.ScrollViewerAttach(   AutoHide$( False??6f   ) ?? AlternationCount$) 2??6?   * ? IsSharedSizeScope$* False??6?     InperBaseListboxStyle#??[ 6?   #??[ 5?      ??5?   C   ?? *
ChartDatas+??5?      ?? 5?      .R?5?   "   A? + A? Columns$+ 1??6.   5?      5?      5?      5?      5?      ? 5?   
   $??1??6   y?6   ?? , ?? Source#, [  - ?? Path$- IsExistEvent?  boolToVisibilityPositive# [ .+?5?      ? -   5        $ fixedBottom??5?      
?????? . J? 	MouseDown. 6(     *Grid_MouseDown+
 5?      ?? 5?      $??40 ?6'   ?? 5?      $??40 ?6'   ?? 5?      $??* ?6'   .+?5?      ?? $??3??6   
???????$ 1Q?6A   $??0 1 0 1q?6M   ?? 5?      $??0??6   
???????$??0 0 1 0q?6=   ._?5?      ?? $
??Wrap=?6    $??Event??64   +  %HandyControl.Controls.TextBlockAttach/   AutoTooltip$
/ True??6B   $??CharacterEllipsis=?6i   $??Center=?6?   $	??3 0q?6?   $??Center=?6?    5?      ?? 5?      $ 90Q?6.   5?      5?      ? 5?      $??1??6   .+?5?      ?? 
???????$??0 0 1 0q?61   ?? 5?      $??Center=?6!   $??Center=?6<   $??10 20q?6Y   .+?5?      ?? $??0 10 0 0q?6"   #??[ 64   $??20??6]   $??20??6h   .??5?      F? ???#1875F0$ Uniform=?62   ?????0????????????????NZ?????%`?k????$7????`????????%????NZ???????ѐNZ?????%??????`?????$7Ӏ??`?k?%????NZ??????????????0??????xH`?@Ys?$7@Ys?`?k@Ys????xH???0?????????0????ӰxH???@Ys`?k@Ys?$7?@Ys`??xH???0??????$7    ???U    ??U?$7?    ?r??U?In
?$7?In
??r?In
?In
?r?In
?$7S?In
أp=
?C@?r?$7?PQ`PQ`??G?z?`@`s??|0 	@e???U	?]???(?Z@00?	 ?????	??6???	?`????	?z?G?P@P~?	@Hy??U	Sp???(\?c@P?v`s?0\PQ`S??G?z?`@P?Q?|?&@e?SP}? ]???(?Z@??  ???? ??6??? `?0/? ?z?G?P@?&@Hy?P?Qp? :?P?v0\0\?@~v?p?P?Q@Hy?&? FP}? `??? ??6?? ? ???? ?gg0/? @e??&?أp=
g_@P?Q0? :?PQ`0\?p??@~v0 	p???U	@Hy?00?	 F???	`????	??6Ӡ??	 ??P~?	?gg??U	@e??0 	 ?|`s?0?PQ`PQ`	6D   5?      ?? 5?      $??0 10 0 0q?6"   #??[ 64   $??20??6]   $??20??6h   $??Center=?6t   .??5?      F? ???#1875F0$ Uniform=?62   ?????    ??v?@x}??v?@x}0>??p?{0>??p?{?hx?    ?hx?    ??v?@x}@x}?    @x}?    p?{?p?{p?{?p?{    ?@x}    ?@x}    @x}@x}@x}@x}??hx0>????v0>????v??v?0>???v?0>??hx??hx?hx??hx0>????v@x}???v    ??hx    ??hxp?{?0>?p?{?0>?@x}???v@x}	6D   5?      ?? 5?      $
??0 10q?6"   #??[ 60   $??20??6Y   $??20??6d   .??5?      F? ???#1875F0$ Uniform=?62   #??[ 6D   5?      5?      5?      ? 5?      $??2??6   .+?5?        -   5        $ scichartbottom??5?   ?   
?????? $ SyncedChartsSameAxisSizes??6B   0 ? DataContext0 6?   ?? *EventChannelChart+$	 PropertyChanged=? 6?     *RenderableSeries+$ PropertyChanged=? 6D  ?? *EventTimeSpanAxis+$	 PropertyChanged=?1   Annotations1 6?  B  <SciChart.Charting.Common.MarkupExtensions.AnnotationsBinding  *Annotations+2   UpdateSourceTrigger$2 PropertyChanged=?3   YAxis3 5?      	  5?      $ False??6-   # [ 6F   $ False??5?   %   $! #0.000??6@   $ False??5?   %   $  6??6@   $??0 0 1 0q?6Q   
???????$??Right=?6   $
" Left=?6?   $# Never=?6?   $
$ -1,2 6
  4 
  VisibleRangeChanged4 6?     *!NumericAxis_VisibleRangeChanged+6
  5?      5?      5?      5?      ?? 5?   
   $??2??6   $??0 1 0 1q?6    	 InperBorderColor#??[	 68   ._?5?      ? 
 5?      ?? 5?      $	??139 ?6'   ?? 5?      $??* ?6'   .+?5?      A? 5 A? Rows$5 2??6   $??0??6'   6 K? ZIndex$	6 999??67    
 InperDialogContentColor#??[
 6J   .+?5?      ?? $??24??6   $	??100??6+   $??24??67   $	??3 0q?6F   $??12(?6T   7 ?? SelectedIndex$7 0??6?   $??Center=?6?   $??Center=?6?   8 ?? SelectionChanged8 6b     *TextFormat_SelectionChanged+??6?   ?? *TextLableFormatting+6?   ? 5?      $??Center=?6    $??Center=?6;   $	??0 7q?6X   .+?5?      ?? #??[ 6"   $??20??6K   $??20??6V   $	??Add??6?   ??6b     *TextFormatChanged+.??5?      F? ???#1875F0$ Uniform=?62   ?????0????????????????NZ?????%`?k????$7????`????????%????NZ???????ѐNZ?????%??????`?????$7Ӏ??`?k?%????NZ??????????????0??????xH`?@Ys?$7@Ys?`?k@Ys????xH???0?????????0????ӰxH???@Ys`?k@Ys?$7?@Ys`??xH???0??????$7    ???U    ??U?$7?    ?r??U?In
?$7?In
??r?In
?In
?r?In
?$7S?In
أp=
?C@?r?$7?PQ`PQ`??G?z?`@`s??|0 	@e???U	?]???(?Z@00?	 ?????	??6???	?`????	?z?G?P@P~?	@Hy??U	Sp???(\?c@P?v`s?0\PQ`S??G?z?`@P?Q?|?&@e?SP}? ]???(?Z@??  ???? ??6??? `?0/? ?z?G?P@?&@Hy?P?Qp? :?P?v0\0\?@~v?p?P?Q@Hy?&? FP}? `??? ??6?? ? ???? ?gg0/? @e??&?أp=
g_@P?Q0? :?PQ`0\?p??@~v0 	p???U	@Hy?00?	 F???	`????	??6Ӡ??	 ??P~?	?gg??U	@e??0 	 ?|`s?0?PQ`PQ`	6D   5?      8  2InperStudioControlLib.Control.TextBox.InperTextBox  5?        TextBoxBaseStyle#??[ 6-   9   InperTextType$9 Double=?6W   :   InperMinValue$: 1Q?6n   ;   InperMaxValue$
; 9999Q?6?   <   InperVerify$
< True??6?   $??45??6?   $??25??6?   $??25??6?   $	??5 0q?6?   $??0q?6?   $??Center=?6?   ??6  ?? *VisibleValue+$% TwoWay=?$	 PropertyChanged=??? 5?      #??[ 6"   $??20??6K   $??20??6V   $??Reduce??6?   ??6b     *TextFormatChanged+.??5?      F? ???#1875F0$ Uniform=?62   #??[ 6D   5?      5?      5?      A? 5?      $5 2??6   $??1??6'   0 67   ?? *EventChannelChart+$	 PropertyChanged=?.+?5?      ?? $??1 0 0 1q?6   $??Stretch=?67   $??Stretch=?6S   #??[	 6q   ._?5?        -   5        $??timesAxisSci??5?   -   $??	-60 0 0 0q?6A   3  -SciChart.Charting.Visuals.SciChartSurfaceBase=   ClipModifierSurface$
= True??6T   
?????? C  =SciChart.Charting.Common.AttachedProperties.PerformanceHelper>   EnableExtremeResamplers$
> True??6?   $ SyncedChartsSameAxisSizes??6?   $??1??6?    6  ?? *ViewportManager+$	 PropertyChanged=? 6]  ?? *TimeSpanAxis+$	 PropertyChanged=?3 5?      	  5?   "   $ False??61   $ False??5?   %   $! #0.000??6B   $ False??5?   %   $
" Left=?6A   $y?	Collapsed=?6V   5?      5?      ?? 5?      $??1 0 0 0q?6   #??[	 67   ._?5?        -   5        $??	sciScroll??5?   p   $??1??6?   $??20??6?   ???	#33000000$
??6 0 q?6?   ?   SelectedRangeChanged? 6/     * SciScroll_SelectedRangeChanged+@ ?? MouseDoubleClick@ 6?     *SciScroll_MouseDoubleClick+{?6?   ?? #, [  $- IsAllowDragScroll?$	 PropertyChanged=?5?      5?      5?      5?      5?      5?      