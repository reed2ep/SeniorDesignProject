using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Point = System.Drawing.Point;

public enum Limb
{
    RightHand,
    LeftHand,
    RightLeg,
    LeftLeg
}

public class Move
{
    public Limb Limb { get; set; }
    public Rectangle From { get; set; }
    public Rectangle To { get; set; }
}

public class MoveCandidate
{
    public Limb Limb { get; set; }
    public Rectangle From { get; set; }
    public Rectangle To { get; set; }
    public double Score { get; set; }
}

public static class RoutePlanner
{
    public static List<Move> PlanSequentialRoute(
        List<Rectangle> holds,
        LimbConfiguration startConfig,
        Rectangle rightHandFinish,
        Rectangle? leftHandFinish,
        double maxReach,
        double targetGap) // targetGap in pixels (0.75 * climberHeightPixels)
    {
        List<Move> moves = new List<Move>();
        LimbConfiguration current = new LimbConfiguration
        {
            RightHand = startConfig.RightHand,
            LeftHand = startConfig.LeftHand,
            RightLeg = startConfig.RightLeg,
            LeftLeg = startConfig.LeftLeg
        };

        int iterations = 0;
        int maxIterations = 100;
        // Get middle of rectangle
        Func<Rectangle, Point> GetCenter = rect => new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

        // Parameters for scoring.
        double thresholdGap = 50;         // When gap between feet and hands exceeds this, adjustments kick in.
        double penaltyFactorHand = 0.5;     // Extra penalty per pixel for hand moves if gap is high.
        double bonusFactorFoot = 0.5;       // Bonus per pixel for foot moves when gap is high.
        double minMoveDistance = 5;         // Ignore moves that are too small.
        double horizontalPenaltyFactor = 0.3; // Penalty for large sideways movements
        double finishBonus = 20;           // Bonus for reaching finish
        double maxHandStep = 30;            // Maximum vertical progress for hand moves.
        double maxFootStep = 20;            // Maximum vertical progress for foot moves.
        int minHandFootGap = 10;            // Minimum gap (in pixels) required between a candidate foot and each hand.

        double ratioPenaltyFactor = 0.5;    // General penalty per pixel deviation from targetGap (applies to both hands and feet).
        double extraHandPenaltyFactor = 1.0;  // Penalty applied when hands move to far from feet
        double extraFootPenaltyFactor = 1.5;  // Penalty applied when feet move to far from hands

        while (!current.HandsAtFinish(rightHandFinish, leftHandFinish) && iterations < maxIterations)
        {

            double footBaseline = Math.Max(current.RightLeg.Y, current.LeftLeg.Y);
            double handBaseline = Math.Min(current.RightHand.Y, current.LeftHand.Y);
            double gap = footBaseline - handBaseline;

            // Separate candidate lists for hand and foot moves.
            List<MoveCandidate> handCandidates = new List<MoveCandidate>();
            List<MoveCandidate> footCandidates = new List<MoveCandidate>();

            foreach (Limb limb in Enum.GetValues(typeof(Limb)))
            {
                Rectangle currentHold = GetLimbHold(current, limb);
                Point centerCurrent = GetCenter(currentHold);
                foreach (var candidate in holds)
                {
                    double dist = Math.Sqrt(
                        Math.Pow(GetCenter(candidate).X - centerCurrent.X, 2) +
                        Math.Pow(GetCenter(candidate).Y - centerCurrent.Y, 2));
                    if (dist < minMoveDistance)
                        continue;

                    if (dist <= maxReach)
                    {
                        double verticalProgress = currentHold.Y - candidate.Y; // positive if candidate is higher.
                        double horizontalDiff = Math.Abs(GetCenter(candidate).X - centerCurrent.X);
                        double score = 0.0;
                        double newGapCandidate = gap; // Simulate new gap if move is applied.

                        if (limb == Limb.RightHand || limb == Limb.LeftHand)
                        {
                            if (verticalProgress < 10)
                                continue;
                            double weight = 2.97;
                            score = weight * verticalProgress - dist;
                            score -= horizontalPenaltyFactor * horizontalDiff;

                            if (candidate == rightHandFinish ||
                               (leftHandFinish.HasValue && candidate == leftHandFinish.Value))
                            {
                                score += finishBonus;
                            }
                            if (verticalProgress > maxHandStep)
                            {
                                double excess = verticalProgress - maxHandStep;
                                double handOvershootPenalty = 1.0;
                                score -= excess * handOvershootPenalty;
                            }
                            // Get new hand baseline
                            double otherHandY = (limb == Limb.RightHand) ? current.LeftHand.Y : current.RightHand.Y;
                            double newHandBaseline = Math.Min(candidate.Y, otherHandY);
                            newGapCandidate = footBaseline - newHandBaseline;
                            if (newGapCandidate > targetGap)
                            {
                                double extra = newGapCandidate - targetGap;
                                score -= extra * extraHandPenaltyFactor;
                            }
                            if (score >= 0)
                                handCandidates.Add(new MoveCandidate { Limb = limb, From = currentHold, To = candidate, Score = score });
                        }
                        else // Foot moves.
                        {
                            // Ensure candidate foot move is at least minHandFootGap below both hands.
                            if (candidate.Y < current.RightHand.Y + minHandFootGap || candidate.Y < current.LeftHand.Y + minHandFootGap)
                                continue;
                            if (verticalProgress < 0)
                                continue;
                            double weight = 1.0;
                            score = weight * verticalProgress - dist;
                            score -= horizontalPenaltyFactor * horizontalDiff;
                            if (verticalProgress > maxFootStep)
                            {
                                double excess = verticalProgress - maxFootStep;
                                double footOvershootPenalty = 1.0;
                                score -= excess * footOvershootPenalty;
                            }
                            double otherFootY = (limb == Limb.RightLeg) ? current.LeftLeg.Y : current.RightLeg.Y;
                            double newFootBaseline = Math.Max(candidate.Y, otherFootY);
                            newGapCandidate = newFootBaseline - handBaseline;
                            if (gap > thresholdGap)
                            {
                                score += (gap - thresholdGap) * bonusFactorFoot;
                            }
                            // Extra penalty if the new gap is less than targetGap (feet too close to hands).
                            if (newGapCandidate < targetGap)
                            {
                                double extra = targetGap - newGapCandidate;
                                score -= extra * extraFootPenaltyFactor;
                            }
                            if (score >= 0)
                                footCandidates.Add(new MoveCandidate { Limb = limb, From = currentHold, To = candidate, Score = score });
                        }
                    }
                }
            }

            // Select candidate: prioritize hand moves if available.
            MoveCandidate bestCandidate = null;
            if (handCandidates.Count > 0)
            {
                bestCandidate = handCandidates.OrderByDescending(c => c.Score).First();
            }
            else if (footCandidates.Count > 0)
            {
                bestCandidate = footCandidates.OrderByDescending(c => c.Score).First();
            }
            else
            {
                break;
            }

            switch (bestCandidate.Limb)
            {
                case Limb.RightHand:
                    current.RightHand = bestCandidate.To;
                    break;
                case Limb.LeftHand:
                    current.LeftHand = bestCandidate.To;
                    break;
                case Limb.RightLeg:
                    current.RightLeg = bestCandidate.To;
                    break;
                case Limb.LeftLeg:
                    current.LeftLeg = bestCandidate.To;
                    break;
            }
            moves.Add(new Move { Limb = bestCandidate.Limb, From = bestCandidate.From, To = bestCandidate.To });
            iterations++;
        }

        return moves;
    }

    private static Rectangle GetLimbHold(LimbConfiguration config, Limb limb)
    {
        switch (limb)
        {
            case Limb.RightHand: return config.RightHand;
            case Limb.LeftHand: return config.LeftHand;
            case Limb.RightLeg: return config.RightLeg;
            case Limb.LeftLeg: return config.LeftLeg;
            default: throw new ArgumentException("Invalid limb");
        }
    }
}

public class LimbConfiguration
{
    public Rectangle RightHand { get; set; }
    public Rectangle LeftHand { get; set; }
    public Rectangle RightLeg { get; set; }
    public Rectangle LeftLeg { get; set; }

    // Require that both hands are on finish holds.
    public bool HandsAtFinish(Rectangle rightFinish, Rectangle? leftFinish)
    {
        if (leftFinish.HasValue)
            return (RightHand == rightFinish && LeftHand == leftFinish.Value);
        else
            return (RightHand == rightFinish && LeftHand == rightFinish);
    }
}

