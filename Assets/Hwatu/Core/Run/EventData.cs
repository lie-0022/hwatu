using System;
using System.Collections.Generic;

namespace Hwatu.Core.Run
{
    /// <summary>이벤트 선택지: 라벨 + 결과 문구 + RunState 변경(효과).</summary>
    public sealed class EventChoice
    {
        private readonly Action<RunState> _apply;
        public string Label { get; }
        public string Result { get; }

        public EventChoice(string label, string result, Action<RunState> apply)
        {
            Label = label;
            Result = result;
            _apply = apply;
        }

        /// <summary>선택 효과를 RunState에 적용한다.</summary>
        public void Apply(RunState run)
        {
            _apply?.Invoke(run);
        }
    }

    /// <summary>이벤트 노드 1개: 제목 + 설명 + 선택지들.</summary>
    public sealed class EventData
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public IReadOnlyList<EventChoice> Choices { get; }

        public EventData(string id, string title, string description, IReadOnlyList<EventChoice> choices)
        {
            Id = id;
            Title = title;
            Description = description;
            Choices = choices;
        }
    }
}
