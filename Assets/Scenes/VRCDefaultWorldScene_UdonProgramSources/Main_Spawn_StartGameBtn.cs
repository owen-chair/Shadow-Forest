
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Main_Spawn_StartGameBtn : UdonSharpBehaviour
{
    private float m_LastPressedTime = 0f;
    private float m_Cooldown = 1f; // Cooldown period in seconds

    public GameObject m_Game;
    public GameObject m_OtherButton;

    public int m_Theme;

    void Start()
    {
        if(this.gameObject != null)
        {
            this.gameObject.SetActive(false);
        }
    }

    public override void Interact()
    {
        if (Time.time - this.m_LastPressedTime < this.m_Cooldown)
        {
            return; // Exit if the button was pressed within the cooldown period
        }

        this.m_LastPressedTime = Time.time; // Update the last pressed time

        base.Interact();

        if(this.m_Game != null)
        {
            Game g = this.m_Game.GetComponent<Game>();
            if(g != null)
            {
                this.gameObject.SetActive(false);
                if(this.m_OtherButton != null)
                {
                    this.m_OtherButton.SetActive(false);
                }
                g.On_StartGameButtonPressed(this.m_Theme);
            }
        }
    }
}
