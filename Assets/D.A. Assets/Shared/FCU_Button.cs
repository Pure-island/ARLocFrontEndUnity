/*
The MIT License (MIT)

Copyright (c) 2014, Unity Technologies

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DA_Assets.FCU.UI
{
    [AddComponentMenu("UI/FCU Button", 30)]
    [RequireComponent(typeof(RectTransform))]
    [Obsolete("Will be replaced in the future versions of asset.")]
    public class FCU_Button : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [Serializable]
        public class ButtonClickedEvent : UnityEvent { }
        [SerializeField, FormerlySerializedAs("onClick")]
        ButtonClickedEvent _onClick = new ButtonClickedEvent();
        public MaskableGraphic buttonText;

        public Color textDefaultColor;
        public Color textHoverColor;
        public Color textPressedColor;
        public Color textSelectedColor;
        public Color textDisabledColor;

        public ButtonClickedEvent onClick
        {
            get { return _onClick; }
            set { _onClick = value; }
        }

        private void Press()
        {
            if (IsActive() == false || IsInteractable() == false)
            {
                return;
            }

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            _onClick.Invoke();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            Press();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (IsActive() == false || IsInteractable() == false)
            {
                return;
            }

            Press();

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        private IEnumerator OnFinishSubmit()
        {
            float fadeTime = colors.fadeDuration;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (base.transition == Transition.SpriteSwap)
            {
                if (interactable == true && buttonText != null)
                {
                    buttonText.color = textPressedColor;
                }
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (base.transition == Transition.SpriteSwap)
            {
                if (interactable == true && buttonText != null)
                {
                    buttonText.color = textDefaultColor;
                }
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (base.transition == Transition.SpriteSwap)
            {
                if (interactable == true && buttonText != null)
                {
                    buttonText.color = textHoverColor;
                }
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (base.transition == Transition.SpriteSwap)
            {
                if (interactable == true && buttonText != null)
                {
                    buttonText.color = textDefaultColor;
                }
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (base.transition == Transition.SpriteSwap)
            {
                if (interactable == true && buttonText != null)
                {
                    buttonText.color = textSelectedColor;
                }
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            if (base.transition == Transition.SpriteSwap)
            {
                if (interactable == true && buttonText != null)
                {
                    buttonText.color = textDefaultColor;
                }
            }
        }

        public override void Select()
        {
            base.Select();
        }

        public override bool IsInteractable()
        {
            bool _interactable = base.IsInteractable();

            if (buttonText == null)
            {
                return _interactable;
            }

            if (_interactable)
            {
                buttonText.color = textDefaultColor;
            }
            else
            {
                buttonText.color = textDisabledColor;
            }

            return _interactable;
        }
    }
}