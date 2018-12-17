using System;

namespace RPICSIO
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// An enum to define the possible PWM ports in the RPICSIO Library.
    /// 
    /// Be aware that you need to ensure the PWM port is configured in /boot/config.txt
    /// before this code will work. This is done by including one of the statements
    /// 
    /// dtoverlay=pwm-with-clk
    /// 
    /// or
    /// 
    /// dtoverlay=pwm-2chan-with-clk 
    /// 
    /// You also need to disable the audio (it conflicts with the PWM system) by 
    /// commenting out the line in the config.txt as shown below
    /// #dtparam=audio=on
    /// 
    /// The pwm-with-clk statement will give you PWM0 on GPIO_18 which is Pin12 on the 
    /// RPi2 Header. The pwm-2chan-with-clk will give you the same PWM0 but also PWM1
    /// which will be available on GPIO_19 which is Pin35 on the RPi2 header.
    /// 
    /// NOTE: these two overlays do not ship with the default raspbian (at this time)
    ///       you will need to download them from
    /// 
    ///   http://www.jumpnowtek.com/rpi/Using-the-Raspberry-Pi-Hardware-PWM-timers.html
    ///
    ///    without these drivers the PWM subsystem will not work. You cannot use the 
    ///    drivers that ship with the default raspbian. They do not enable an internal
    ///    clock.
    /// 
    ///    Always set the frequency first then the Pulse Width/duty cycle. The pulse 
    ///    width is calculated from whatever frequency is currently set it is
    ///    not adjusted if the frequency is later changed.
    /// 
    /// </summary>
    /// <history>
    ///    01 Dec 16  Cynic - Originally written
    /// </history>
    public enum PWMPortEnum
    {
        PWM_NONE,
        PWM_0,       // output pin depends on dt_overlay
                     //    GPIO_18, header Pin12 by default
        PWM_1,       // output pin depends on dt_overlay       
                     //    GPIO_19, header Pin35 by default
    }
}

