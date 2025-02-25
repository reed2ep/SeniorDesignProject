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
        double maxReach)
    {
        List<Move> moves = new List<Move>();
        // Initialize current configuration.
        LimbConfiguration current = new LimbConfiguration
        {
            RightHand = startConfig.RightHand,
            LeftHand = startConfig.LeftHand,
            RightLeg = startConfig.RightLeg,
            LeftLeg = startConfig.LeftLeg
        };

        int iterations = 0;
        int maxIterations = 100;
        // Helper: center point of a rectangle.
        Func<Rectangle, Point> GetCenter = rect => new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

        // Parameters for candidate scoring.
        double thresholdGap = 50;      // gap (in pixels) for extra adjustments.
        double penaltyFactorHand = 0.5;  // extra penalty per pixel if hand move is too high.
        double bonusFactorFoot = 0.5;    // bonus per pixel for foot moves when gap is large.
        double minMoveDistance = 5;      // ignore moves that barely change position.
        double horizontalPenaltyFactor = 0.3; // penalize moves with excessive horizontal deviation.
        double finishBonus = 100;        // bonus score if candidate is exactly a finish hold.

        while (!current.HandsAtFinish(rightHandFinish, leftHandFinish) && iterations < maxIterations)
        {
            // For foot moves, use the lower hand as a baseline.
            // In screen coordinates, a larger Y means lower.
            double handBaselineForFeet = Math.Max(current.RightHand.Y, current.LeftHand.Y);

            // (For other purposes, you might compute a gap between feet and hands.)
            double footBaseline = Math.Max(current.RightLeg.Y, current.LeftLeg.Y);
            double handBaseline = Math.Min(current.RightHand.Y, current.LeftHand.Y);
            double gap = footBaseline - handBaseline;

            List<MoveCandidate> candidates = new List<MoveCandidate>();

            // Evaluate candidate moves for each limb.
            foreach (Limb limb in Enum.GetValues(typeof(Limb)))
            {
                Rectangle currentHold = GetLimbHold(current, limb);
                Point centerCurrent = GetCenter(currentHold);
                foreach (var candidate in holds)
                {
                    // Skip if candidate is occupied.
                    if (candidate == current.RightHand ||
                        candidate == current.LeftHand ||
                        candidate == current.RightLeg ||
                        candidate == current.LeftLeg)
                        continue;

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

                        if (limb == Limb.RightHand || limb == Limb.LeftHand)
                        {
                            if (verticalProgress < 10)
                                continue;
                            double weight = 3.0;
                            score = weight * verticalProgress - dist;
                            score -= horizontalPenaltyFactor * horizontalDiff;

                            // Bonus if candidate is exactly a finish hold.
                            if (candidate == rightHandFinish || (leftHandFinish.HasValue && candidate == leftHandFinish.Value))
                            {
                                score += finishBonus;
                            }

                            if (gap > thresholdGap)
                            {
                                score -= (gap - thresholdGap) * penaltyFactorHand;
                            }
                        }
                        else // Foot moves.
                        {
                            // Prevent feet from moving above either hand.
                            if (candidate.Y < handBaselineForFeet)
                                continue;
                            if (verticalProgress < 0)
                                continue;
                            double weight = 1.0;
                            score = weight * verticalProgress - dist;
                            score -= horizontalPenaltyFactor * horizontalDiff;

                            if (gap > thresholdGap)
                            {
                                score += (gap - thresholdGap) * bonusFactorFoot;
                            }
                        }

                        if (score >= 0)
                        {
                            candidates.Add(new MoveCandidate
                            {
                                Limb = limb,
                                From = currentHold,
                                To = candidate,
                                Score = score
                            });
                            Console.WriteLine($"Candidate for {limb}: from {currentHold} to {candidate} | Score: {score:F2}");
                        }
                    }
                }
            }

            if (candidates.Count == 0)
            {
                // No valid moves found; exit loop.
                break;
            }

            var best = candidates.OrderByDescending(c => c.Score).First();

            switch (best.Limb)
            {
                case Limb.RightHand:
                    current.RightHand = best.To;
                    break;
                case Limb.LeftHand:
                    current.LeftHand = best.To;
                    break;
                case Limb.RightLeg:
                    current.RightLeg = best.To;
                    break;
                case Limb.LeftLeg:
                    current.LeftLeg = best.To;
                    break;
            }
            moves.Add(new Move { Limb = best.Limb, From = best.From, To = best.To });
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

    // Now require that both hands are on finish holds.
    public bool HandsAtFinish(Rectangle rightFinish, Rectangle? leftFinish)
    {
        if (leftFinish.HasValue)
            return (RightHand == rightFinish && LeftHand == leftFinish.Value);
        else
            return (RightHand == rightFinish && LeftHand == rightFinish);
    }
}

