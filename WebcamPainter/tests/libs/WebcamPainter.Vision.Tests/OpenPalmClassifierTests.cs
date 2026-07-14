using CodeBrix.VideoProcessing.OpenCV5;
using SilverAssertions;
using Xunit;
using WebcamPainter.Vision.Internal;

namespace WebcamPainter.Vision.Tests;

public class OpenPalmClassifierTests
{
    //MediaPipe hand topology: 0 = wrist; thumb 1-4; then per finger MCP/PIP/DIP/TIP
    //  (index 5-8, middle 9-12, ring 13-16, pinky 17-20)
    private static Point2f[] CreateHand(float tipY)
    {
        var landmarks = new Point2f[21];
        landmarks[0] = new Point2f(100, 300);                        //wrist

        landmarks[1] = new Point2f(70, 280);                         //thumb
        landmarks[2] = new Point2f(55, 260);
        landmarks[3] = new Point2f(45, 245);
        landmarks[4] = new Point2f(38, 232);

        float[] fingerX = { 80, 95, 110, 125 };                      //index/middle/ring/pinky columns
        for (int finger = 0; finger < 4; finger++)
        {
            int mcp = 5 + (finger * 4);
            landmarks[mcp] = new Point2f(fingerX[finger], 220);      //MCP knuckle
            landmarks[mcp + 1] = new Point2f(fingerX[finger], 190);  //PIP
            landmarks[mcp + 2] = new Point2f(fingerX[finger], (190 + tipY) / 2);  //DIP
            landmarks[mcp + 3] = new Point2f(fingerX[finger], tipY); //TIP
        }
        return landmarks;
    }

    [Fact]
    public void IsOpenPalm_true_when_all_fingers_extended()
    {
        //Arrange - fingertips far above the PIP joints (hand pointing up)
        Point2f[] landmarks = CreateHand(tipY: 120);

        //Assert
        OpenPalmClassifier.IsOpenPalm(landmarks).Should().Be(true);
    }

    [Fact]
    public void IsOpenPalm_false_when_fingers_curled()
    {
        //Arrange - fingertips folded back level with the knuckles (a fist)
        Point2f[] landmarks = CreateHand(tipY: 225);

        //Assert
        OpenPalmClassifier.IsOpenPalm(landmarks).Should().Be(false);
    }

    [Fact]
    public void IsOpenPalm_false_when_one_finger_curled()
    {
        //Arrange - open hand, but the index fingertip is pulled back to its knuckle
        Point2f[] landmarks = CreateHand(tipY: 120);
        landmarks[8] = new Point2f(80, 218);

        //Assert
        OpenPalmClassifier.IsOpenPalm(landmarks).Should().Be(false);
    }

    [Fact]
    public void IsOpenPalm_false_for_missing_landmarks()
    {
        OpenPalmClassifier.IsOpenPalm(null).Should().Be(false);
        OpenPalmClassifier.IsOpenPalm(new Point2f[5]).Should().Be(false);
    }

    [Fact]
    public void GetPalmCenter_averages_wrist_and_knuckles()
    {
        //Arrange
        Point2f[] landmarks = CreateHand(tipY: 120);

        //Act
        Point2f center = OpenPalmClassifier.GetPalmCenter(landmarks);

        //Assert - mean of wrist (100, 300) and the four MCPs (80|95|110|125, 220)
        center.X.Should().Be((100f + 80 + 95 + 110 + 125) / 5f);
        center.Y.Should().Be((300f + 220 + 220 + 220 + 220) / 5f);
    }
}
