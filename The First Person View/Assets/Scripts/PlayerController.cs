using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float h = 0.0f;
    float v = 0.0f;

    float moveSpeed = 10.0f;
    Vector3 moveDir = Vector3.zero;

    public float rotSpeed = 300.0f;
    Vector3 m_CacVec = Vector3.zero;

    void Update()
    {
        //-- �̵�����
        h = Input.GetAxis("Horizontal");    //-1.0f ~ 1.0f
        v = Input.GetAxis("Vertical");      //-1.0f ~ 1.0f

        //�����¿� �� ���� ���� ���
        moveDir = (Vector3.forward * v) + (Vector3.right * h);
        moveDir.Normalize();    //���� ���ͷ� ���

        //Transtlate(�̵����� * Time.deltaTime * �ӵ�, ������ǥ);
        transform.Translate(moveDir * Time.deltaTime * moveSpeed, Space.Self);
        // Space.Self ������ �⺻ ���� ���� ��ǥ�̴�.
        //-- �̵�����

        //-- ī�޶� ȸ�� ����
        if (Input.GetMouseButton(1) == true)
        {   //���콺 ������ ��ư ������ �ִ� ����
            m_CacVec = transform.eulerAngles;
            m_CacVec.y += (rotSpeed * Time.deltaTime * Input.GetAxis("Mouse X"));
            m_CacVec.x += (rotSpeed * Time.deltaTime * Input.GetAxis("Mouse Y"));

            //if (120.0f < m_CacVec.x && m_CacVec.x < 340.0f)
            //    m_CacVec.x = 340.0f;

            //if (m_CacVec.x < 90.0f && 12.0f < m_CacVec.x)
            //    m_CacVec.x = 12.0f;

            transform.eulerAngles = m_CacVec;
        }
        //-- ī�޶� ȸ�� ����
 
    }

    public bool IsMove()
    {
        if (h == 0.0f && v == 0.0f) { return false; }

        return true;
    }
}
