using System;
using MonoBrick.EV3;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
 
public static class Program
{
    enum direction { NORTH, EAST, SOUTH, WEST };
    // Start the orientation from the middle of 0 -> max int so we can
    // subtract this value for counter cw rotations and use modulus to
    // determine the orientation.
    static direction orientation = direction.NORTH + 4 * 10000;
 
    // static int x_pos = 0;
    // static int y_pos = 0;
 
    const int BLUE_COLOR_THRESHOLD = 20;
    const int TACHO_DISTANCE_PER_BLOCK = 485;
    const float ULTRASONIC_DISTANCE_PER_BLOCK = 23;
 
    // The color sensor motor tacho count difference from the neutral
    // position, for the sensor to be at the left most position.
    const int LEFT_POS_COLOR_TACHO_DIFF = 40;
    const int RIGHT_POS_COLOR_TACHO_DIFF = 240;
   
    static int origin_color_sensor_tacho = 0;
 
    static EV3Plus<Sensor, Sensor, Sensor, Sensor> ev3;
 
    static float ReadUltrasonic(){
        return ((UltrasonicSensor)ev3.Sensor4).Read();
    }
 
    // Only works on intersections.
    private static void TurnCounterCW()
    {
        Console.WriteLine("Robot Turning CounterClockWise");
 
        ev3.MotorA.On(10);
        ev3.MotorB.On(-10);
        LockUntilGreaterThanColor(BLUE_COLOR_THRESHOLD);
        LockUntilLessThanColor(BLUE_COLOR_THRESHOLD);
        LockUntilGreaterThanColor(BLUE_COLOR_THRESHOLD);
 
        ev3.Stop();
 
        orientation -= 1;
    }
 
    // Only works on intersections.
    // detect_blue_first: If the turn should be sensing blue -> grey -> blue, instead of grey -> blue
    private static void TurnCW(bool detect_blue_first = true)
    {
        Console.WriteLine("Robot Turning CounterClockWise");
 
        ev3.MotorA.On(-10);
        ev3.MotorB.On(10);
        if (detect_blue_first){
            LockUntilLessThanColor(BLUE_COLOR_THRESHOLD);
        }
        LockUntilGreaterThanColor(BLUE_COLOR_THRESHOLD);
        LockUntilLessThanColor(BLUE_COLOR_THRESHOLD);
 
        ev3.Stop();
 
        orientation += 1;
    }
 
    static int[] GetOnFirstIntersection(){
        float ultrasonic_val = ReadUltrasonic();
        int cell_x_pos = (int)(Math.Round(ultrasonic_val / ULTRASONIC_DISTANCE_PER_BLOCK));
 
        Console.WriteLine(ultrasonic_val);
        Console.WriteLine(cell_x_pos);
        if (cell_x_pos == 0){
            TurnCWWithTacho();
        } else {
            TurnCounterCWWithTacho();
        }
 
        GetOnTrack();
 
        ultrasonic_val = ReadUltrasonic();
        int cell_y_pos = (int)(Math.Round(ultrasonic_val / ULTRASONIC_DISTANCE_PER_BLOCK));
 
        Console.WriteLine(ultrasonic_val);
        Console.WriteLine(cell_y_pos);
        if (cell_y_pos == 0){
            TurnCW(false);
        } else {
            TurnCounterCW();
        }
 
        DetectIntersection();

        LocateRobot(cell_x_pos, cell_y_pos);
 
        return new int[] {cell_x_pos, cell_y_pos};
    }
 
    static void LocateRobot(int cell_x_pos, int cell_y_pos){
        int intersection_x_pos = 0;
        int intersection_y_pos = 0;
        if (cell_y_pos == 0 && cell_x_pos == 0){
            intersection_x_pos = 1;
 
            float ultrasonic_val = ReadUltrasonic();
            int distance = (int)(Math.Round(ultrasonic_val / (ULTRASONIC_DISTANCE_PER_BLOCK / 2) )); // The number of half blocks away from the wall
            if (distance > 7) { // If the robot is more than 3.5 blocks away from the wall
                intersection_y_pos = 3;
            }else{
                intersection_y_pos = 5;
            }
            orientation -= 1;
        }
        else if(cell_y_pos != 0 && cell_x_pos != 0){
            intersection_x_pos = cell_x_pos;
            intersection_y_pos = cell_y_pos;
            orientation -= 1;
        }
        else if(cell_x_pos != 0 && cell_y_pos == 0){
            intersection_x_pos = cell_x_pos;
            intersection_y_pos = 1;
            orientation += 1 ; 
        }
        else if(cell_x_pos == 0 && cell_y_pos != 0){
            const int long_max_cells = 6;
            const int short_max_cells = 4;
            
            if(cell_y_pos > 3){
                intersection_x_pos = 1;
                intersection_y_pos = long_max_cells - cell_y_pos;
                orientation += 1;
            }
            
            else{
                TurnCWWithTacho();    // how many tacho counts is 180 degrees? Pulled this out of another function.
                orientation -= 1;
                float ultrasonic_val = ReadUltrasonic(); 
                int distance = (int)(Math.Round(ultrasonic_val / (ULTRASONIC_DISTANCE_PER_BLOCK / 2) )); // The number of half blocks away from the wall
                if(distance > 4 ){
                    intersection_x_pos = 1;
                    intersection_y_pos = short_max_cells - cell_y_pos;
                }
                else{
                    intersection_x_pos = 1;
                    intersection_y_pos = long_max_cells - cell_y_pos;
                }
            }
        }
        Console.WriteLine("Location detected!");
        Console.WriteLine("x: {0} -- y: {1}", intersection_x_pos, intersection_y_pos);
    }
 
    static void GetOnTrack(){
        Console.WriteLine(origin_color_sensor_tacho);
        ev3.Forward(10);
        LockUntilLessThanColor(BLUE_COLOR_THRESHOLD);
        Int32 init_tacho_count = ev3.MotorA.GetTachoCount();
        ev3.LockUntilTachoGoal(init_tacho_count, 80, ev3.MotorA);
        ev3.Stop();
    }
 
    static void LockUntilLessThanColor(int color_threshold)
    {
        var sensor_val = ((ColorSensor)ev3.Sensor3).Read();
        while (sensor_val > color_threshold)
        {
            Debug.WriteLine(sensor_val);
            Thread.Sleep(10);
            sensor_val = ((ColorSensor)ev3.Sensor3).Read();
        }
    }
 
    static void LockUntilGreaterThanColor(int color_threshold)
    {
        var sensor_val = ((ColorSensor)ev3.Sensor3).Read();
        while (sensor_val < color_threshold)
        {
            Debug.WriteLine(sensor_val);
            Thread.Sleep(10);
            sensor_val = ((ColorSensor)ev3.Sensor3).Read();
        }
    }
 
    static void ScanLine(){
        ev3.MotorC.On(10);
        var max_tacho_distance = 180;
        var new_tacho_count = ev3.MotorC.GetTachoCount();
        var initial_tacho_count = new_tacho_count;
        while (new_tacho_count - initial_tacho_count < max_tacho_distance && new_tacho_count - initial_tacho_count > -1 * max_tacho_distance)
        {
            new_tacho_count = ev3.MotorC.GetTachoCount();
            Debug.WriteLine(new_tacho_count - initial_tacho_count);
            Thread.Sleep(10);
        }
        ev3.MotorC.Off();
    }
 
    static void LineFollow(int max_tacho_distance){
        ev3.Forward(10);
 
        var motor_to_check = ev3.MotorB;
        var new_tacho_count = motor_to_check.GetTachoCount();
        var initial_tacho_count = new_tacho_count;
        while (new_tacho_count - initial_tacho_count < max_tacho_distance && new_tacho_count - initial_tacho_count > -1 * max_tacho_distance)
        {
            if (((ColorSensor)ev3.Sensor3).Read() < BLUE_COLOR_THRESHOLD){
                ev3.MotorA.On(10);
                ev3.MotorB.On(4);
            }else{
                ev3.MotorA.On(4);
                ev3.MotorB.On(10);
            }
            new_tacho_count = motor_to_check.GetTachoCount();
            Debug.WriteLine(new_tacho_count - initial_tacho_count);
            Thread.Sleep(10);
        }
        ev3.Stop();
    }
 
    // Move forward one intersection
    static void MoveForwardOneBlock(){
        LineFollow(400);
        DetectIntersection();
    }
 
    static void MoveSensorLeft()
    {
        ev3.MotorC.On(10);
        var init_tacho = ev3.MotorC.GetTachoCount();
        var tacho_goal = origin_color_sensor_tacho + LEFT_POS_COLOR_TACHO_DIFF - init_tacho;
        ev3.LockUntilTachoGoal(init_tacho, tacho_goal, ev3.MotorC);
        ev3.MotorC.Off();
    }
 
    static void MoveSensorRight()
    {
        ev3.MotorC.On(-10);
        var init_tacho = ev3.MotorC.GetTachoCount();
        var tacho_goal = init_tacho - (origin_color_sensor_tacho - RIGHT_POS_COLOR_TACHO_DIFF);
        ev3.LockUntilTachoGoal(init_tacho, tacho_goal, ev3.MotorC);
        ev3.MotorC.Off();
    }
 
    static void MoveSensorToNeutral(){
        var init_tacho = ev3.MotorC.GetTachoCount();
        if (init_tacho - origin_color_sensor_tacho > 0){
            ev3.MotorC.On(-10);
        }else{
            ev3.MotorC.On(10);
        }
 
        var tacho_goal = Math.Abs(init_tacho - origin_color_sensor_tacho);
        ev3.LockUntilTachoGoal(init_tacho, tacho_goal, ev3.MotorC);
        ev3.MotorC.Off();
    }
 
    static void DetectIntersection(){
        bool right_is_blue = false;
        bool left_is_blue = false;
        while(! (right_is_blue && left_is_blue)){
            MoveSensorRight();
            right_is_blue = ((ColorSensor)ev3.Sensor3).Read() < BLUE_COLOR_THRESHOLD;
 
            MoveSensorLeft();
            left_is_blue = ((ColorSensor)ev3.
                Sensor3).Read() < BLUE_COLOR_THRESHOLD;
 
            MoveSensorToNeutral();
            LineFollow(20);
        }
        MoveSensorToNeutral();
        LineFollow(30); // After the sensor is on the line, move forward so wheels are on the line.
    }
 
    static void TurnCWWithTacho(){
        ev3.MotorA.On(-10);
        ev3.MotorB.On(10);
 
        var motor_to_check = ev3.MotorB;
        var init_tacho = motor_to_check.GetTachoCount();
        var tacho_goal = 180;
        ev3.LockUntilTachoGoal(init_tacho, tacho_goal, motor_to_check);
        ev3.Stop();
    }
 
    static void TurnCounterCWWithTacho(){
        ev3.MotorA.On(10);
        ev3.MotorB.On(-10);
 
        var motor_to_check = ev3.MotorB;
        var init_tacho = motor_to_check.GetTachoCount();
        var tacho_goal = 180;
        ev3.LockUntilTachoGoal(init_tacho, tacho_goal, motor_to_check);
        ev3.Stop();
    }
 
    static void Main(string[] args)
    {
        ev3 = new EV3Plus<Sensor, Sensor, Sensor, Sensor>("COM3");
        ((Brick<Sensor, Sensor, Sensor, Sensor>)ev3).Connection.Close();
        ((Brick<Sensor, Sensor, Sensor, Sensor>)ev3).Connection.Open();
 
        ev3.Sensor4 = new UltrasonicSensor();
        ev3.Sensor3 = new ColorSensor(ColorMode.Reflection);
 
        ConsoleKeyInfo cki;
        Console.WriteLine("Press Q to quit");
 
        origin_color_sensor_tacho = ev3.MotorC.GetTachoCount();
 
        bool quit = false;
        while (!quit)
        {
            cki = Console.ReadKey(true); // press a key    
            switch (cki.Key)
            {
               
                case ConsoleKey.T:
                    Console.WriteLine(ReadUltrasonic());
                    break;
 
                case ConsoleKey.Y:  // prints the value from the colour sensor
                    Console.WriteLine(((ColorSensor)ev3.Sensor3).Read());
                    break;
               
                case ConsoleKey.G:
                    TurnCounterCW();
                    break;
               
                case ConsoleKey.J:
                    TurnCWWithTacho();
                    break;
               
                case ConsoleKey.H:
                    TurnCW();
                    break;
               
                case ConsoleKey.P:
                    TurnCW(false);
                    break;
 
                case ConsoleKey.A:
                    LineFollow(225);
                    break;
 
                case ConsoleKey.D:
                    MoveForwardOneBlock();
                    break;
 
                case ConsoleKey.I:
                    DetectIntersection();
                    break;
 
                case ConsoleKey.L:
                    LocateRobot(1, 3);
                    break;
 
                case ConsoleKey.C:
                    ev3.MotorC.On(10);
                    ev3.LockUntilTachoGoal(ev3.MotorC.GetTachoCount(), 5, ev3.MotorC);
                    ev3.MotorC.Off();
                    break;
 
                case ConsoleKey.V:
                    ev3.MotorC.On(-10);
                    ev3.LockUntilTachoGoal(ev3.MotorC.GetTachoCount(), 5, ev3.MotorC);
                    ev3.MotorC.Off();
                    break;
               
                case ConsoleKey.W:
                    MoveSensorLeft();
                    break;
               
                case ConsoleKey.M:
                    Console.WriteLine(ev3.MotorC.GetTachoCount());
                    break;
               
                case ConsoleKey.N:
                    MoveSensorToNeutral();
                    break;
               
                case ConsoleKey.R:
                    MoveSensorRight();
                    break;
               
                case ConsoleKey.B:
                    GetOnTrack();
                    break;
 
                case ConsoleKey.Q:
                    ev3.Stop();
                    quit = true;
                    break;
 
                case ConsoleKey.S:
                    ev3.Stop();
                    ev3.MotorC.Brake();
                    break;
 
                case ConsoleKey.U:
                    GetOnFirstIntersection();
                    break;
            }
        }
        ev3.Connection.Close();
    }
}
 
namespace MonoBrick.EV3
{
    public class EV3Plus<TSensor1, TSensor2, TSensor3, TSensor4> : Brick<TSensor1, TSensor2, TSensor3, TSensor4>
        where TSensor1 : Sensor, new()
        where TSensor2 : Sensor, new()
        where TSensor3 : Sensor, new()
        where TSensor4 : Sensor, new()
    {
        public EV3Plus(string connection) : base(connection) {
        }
 
        public void Rotate(Int32 degrees)
        {
            float TACHO_DISTANCE_PER_DEGREES = 410/360;
            int initial_tacho_count = this.MotorA.GetTachoCount();
            this.MotorA.On(100);
            this.MotorD.On(-100);
 
            var max_tacho_distance = TACHO_DISTANCE_PER_DEGREES * degrees;
            LockUntilTachoGoal(initial_tacho_count, max_tacho_distance, this.MotorA);
 
            this.MotorA.Brake();
            this.MotorD.Brake();
        }
 
        public void LockUntilTachoGoal(Int32 initial_tacho_count, float max_tacho_distance, Motor motor_to_check)
        {
            var new_tacho_count = motor_to_check.GetTachoCount();
            while (new_tacho_count - initial_tacho_count < max_tacho_distance && new_tacho_count - initial_tacho_count > -1 * max_tacho_distance)
            {
                new_tacho_count = motor_to_check.GetTachoCount();
                Debug.WriteLine(new_tacho_count);
                Debug.WriteLine(new_tacho_count - initial_tacho_count);
                Thread.Sleep(10);
            }
        }
        public void ReverseAnd180()
        {
            int initial_tacho_count = this.MotorA.GetTachoCount();
            Forward(-50);
            LockUntilTachoGoal(initial_tacho_count, 280, this.MotorA);
            Rotate(180);
        }
 
        public void Forward(sbyte speed)
        {
            this.MotorA.On(speed);
            this.MotorB.On(speed);
        }
 
       
 
        public void Stop()
        {
            Debug.WriteLine("EV3Plus.Stop()");
            this.MotorA.Brake();
            this.MotorB.Brake();
            this.MotorC.Brake();
            this.MotorA.Off();
            this.MotorB.Off();
            this.MotorC.Off();
        }
    }
}