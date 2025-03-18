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
        List<Hold> holds,
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
        Func<Rectangle, Point> GetCenter = rect => new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

        double thresholdGap = 50;
        double penaltyFactorHand = 0.5;
        double bonusFactorFoot = 0.5;
        double minMoveDistance = 5;
        double horizontalPenaltyFactor = 0.3;
        double finishBonus = 20;
        double maxHandStep = 30;
        double maxFootStep = 20;
        int minHandFootGap = 10;

        double ratioPenaltyFactor = 0.5;
        double extraHandPenaltyFactor = 1.0;
        double extraFootPenaltyFactor = 1.5;

        Limb? lastFootMoved = null;

        while (!current.HandsAtFinish(rightHandFinish, leftHandFinish) && iterations < maxIterations)
        {
            double footBaseline = Math.Max(current.RightLeg.Y, current.LeftLeg.Y);
            double handBaseline = Math.Min(current.RightHand.Y, current.LeftHand.Y);
            double gap = footBaseline - handBaseline;

            List<MoveCandidate> handCandidates = new List<MoveCandidate>();
            List<MoveCandidate> footCandidates = new List<MoveCandidate>();

            foreach (Limb limb in Enum.GetValues(typeof(Limb)))
            {
                Rectangle currentHold = GetLimbHold(current, limb);
                Point centerCurrent = GetCenter(currentHold);
                foreach (var candidateHold in holds)
                {
                    // Use the candidate's Bounds for geometry.
                    Rectangle candidate = candidateHold.Bounds;
                    double dist = Math.Sqrt(
                        Math.Pow(GetCenter(candidate).X - centerCurrent.X, 2) +
                        Math.Pow(GetCenter(candidate).Y - centerCurrent.Y, 2));
                    if (dist < minMoveDistance)
                        continue;

                    if (dist <= maxReach)
                    {
                        double verticalProgress = currentHold.Y - candidate.Y;
                        double horizontalDiff = Math.Abs(GetCenter(candidate).X - centerCurrent.X);
                        double score = 0.0;
                        double newGapCandidate = gap;

                        // Apply a difficulty penalty based on the candidate hold's Difficulty.
                        double difficultyPenalty = candidateHold.Difficulty switch
                        {
                            HoldDifficulty.Medium => 5.0,
                            HoldDifficulty.Hard => 15.0,
                            HoldDifficulty.Extreme => 30.0,
                            _ => 0.0  // Easy
                        };

                        if (limb == Limb.RightHand || limb == Limb.LeftHand)
                        {
                            if (verticalProgress < 10)
                                continue;
                            double weight = 2.97;
                            score = weight * verticalProgress - dist;
                            score -= horizontalPenaltyFactor * horizontalDiff;
                            score -= difficultyPenalty; // Subtract difficulty penalty

                            if (candidate == rightHandFinish ||
                                (leftHandFinish.HasValue && candidate == leftHandFinish.Value))
                            {
                                score += finishBonus;
                            }
                            if (verticalProgress > maxHandStep)
                            {
                                double excess = verticalProgress - maxHandStep;
                                score -= excess * 1.0;
                            }
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
                            if (candidate.Y < current.RightHand.Y + minHandFootGap || candidate.Y < current.LeftHand.Y + minHandFootGap)
                                continue;
                            if (verticalProgress < 0)
                                continue;
                            double weight = 1.0;
                            score = weight * verticalProgress - dist;
                            score -= horizontalPenaltyFactor * horizontalDiff;
                            score -= difficultyPenalty * 0.75; // Apply a reduced penalty for foot holds
                            if (verticalProgress > maxFootStep)
                            {
                                double excess = verticalProgress - maxFootStep;
                                score -= excess * 1.0;
                            }
                            double otherFootY = (limb == Limb.RightLeg) ? current.LeftLeg.Y : current.RightLeg.Y;
                            double newFootBaseline = Math.Max(candidate.Y, otherFootY);
                            newGapCandidate = newFootBaseline - handBaseline;
                            if (gap > thresholdGap)
                            {
                                score += (gap - thresholdGap) * bonusFactorFoot;
                            }
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

            // Enforce alternating foot moves: if last foot moved exists, filter for the opposite foot if possible.
            if (lastFootMoved != null)
            {
                var alternateFootCandidates = footCandidates.Where(c => c.Limb != lastFootMoved).ToList();
                if (alternateFootCandidates.Any())
                {
                    footCandidates = alternateFootCandidates;
                }
            }

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
                    // Reset foot move memory when a hand moves.
                    lastFootMoved = null;
                    break;
                case Limb.LeftHand:
                    current.LeftHand = bestCandidate.To;
                    lastFootMoved = null;
                    break;
                case Limb.RightLeg:
                    current.RightLeg = bestCandidate.To;
                    lastFootMoved = Limb.RightLeg;
                    break;
                case Limb.LeftLeg:
                    current.LeftLeg = bestCandidate.To;
                    lastFootMoved = Limb.LeftLeg;
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

