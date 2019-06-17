# FindObjByPath
一个根据路径寻找子物体的工具。

在制作UI的时候，经常会写大量的重复的寻找物体的代码。
例如：
···c#
content = transform.Find("Content").GetComponent<RectTransform>();
···  
那么有没有办法自动生成呢？

这就是这个工具所做的事情。


